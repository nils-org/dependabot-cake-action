#!/usr/bin/env ruby

require "dependabot/file_fetchers"
require "dependabot/file_parsers"
require "dependabot/update_checkers"
require "dependabot/file_updaters"
require "dependabot/pull_request_creator"
require "dependabot/omnibus"


# Utilize the github env variable per default
repo_name = ENV["GITHUB_REPOSITORY"]
if !repo_name || repo_name.empty?
  print "GITHUB_REPOSITORY needs to be set"
  exit(1)
end

# Directory where the base dependency files are.
directory = ENV["INPUT_DIRECTORY"] || "/"
directory = directory.gsub(/\\n/, "\n")
if directory&.empty?
  print "The directory needs to be set"
  exit(1)
end

# Define the target branch
target_branch = ENV["INPUT_TARGET_BRANCH"]
if target_branch && target_branch.empty?
  target_branch=nil
end

# Token to be used for fetching repository files / creating pull requests
repo_token = ENV["INPUT_TOKEN"]
if !repo_token || repo_token.empty?
  print "A github token needs to be provided"
  exit(1)
end

# DryRun - does not create real PRs
dry_run = ENV["DRY_RUN"] && !ENV["DRY_RUN"].empty?

credentials_repository = [
  {
    "type" => "git_source",
    "host" => "github.com",
    "username" => "x-access-token",
    "password" => repo_token
  }
]

def update(source, credentials_repository, dry_run)

  # Hardcode the package manager to cake
  package_manager = "cake"

  ##############################
  # Fetch the dependency files #
  ##############################
  fetcher = Dependabot::FileFetchers.for_package_manager(package_manager).new(
    source: source,
    credentials: credentials_repository,
  )

  files = fetcher.files
  commit = fetcher.commit

  if (files.empty?)
    puts "    - no files found"
  else
    files.each do |f|
      puts "    - found: #{f.name} "
    end 
  end

  ##############################
  # Parse the dependency files #
  ##############################
  puts "  - Parsing dependencies information"
  parser = Dependabot::FileParsers.for_package_manager(package_manager).new(
    dependency_files: files,
    source: source,
    credentials: credentials_repository,
  )

  dependencies = parser.parse

  dependencies.select(&:top_level?).each do |dep|
    #########################################
    # Get update details for the dependency #
    #########################################
    checker = Dependabot::UpdateCheckers.for_package_manager(package_manager).new(
      dependency: dep,
      dependency_files: files,
      credentials: credentials_repository,
    )

    next if checker.up_to_date?

    requirements_to_unlock =
      if !checker.requirements_unlocked_or_can_be?
        if checker.can_update?(requirements_to_unlock: :none) then :none
        else :update_not_possible
        end
      elsif checker.can_update?(requirements_to_unlock: :own) then :own
      elsif checker.can_update?(requirements_to_unlock: :all) then :all
      else :update_not_possible
      end

    next if requirements_to_unlock == :update_not_possible

    updated_deps = checker.updated_dependencies(
      requirements_to_unlock: requirements_to_unlock
    )

    #####################################
    # Generate updated dependency files #
    #####################################
    puts "  - Updating #{dep.name} (from #{dep.version})"
    updater = Dependabot::FileUpdaters.for_package_manager(package_manager).new(
      dependencies: updated_deps,
      dependency_files: files,
      credentials: credentials_repository,
    )

    updated_files = updater.updated_dependency_files
    updated_files.each do |f|
      puts "    - file:#{f.name}"
    end 
    

    if (dry_run)
      puts "    - dry run (no PR)"
      next
    else
      ########################################
      # Create a pull request for the update #
      ########################################
      pr_creator = Dependabot::PullRequestCreator.new(
        source: source,
        base_commit: commit,
        dependencies: updated_deps,
        files: updated_files,
        credentials: credentials_repository,
        label_language: false,
      )
      pull_request = pr_creator.create
      puts "    - PR submitted: #{pull_request}"
    end

  end
end

puts "  - Fetching dependency files for #{repo_name}"
directory.split("\n").each do |dir|
  puts "  - Checking #{dir} ..."

  begin
    source = Dependabot::Source.new(
      provider: "github",
      repo: repo_name,
      directory: dir.strip,
      branch: target_branch,
    )
    update source, credentials_repository, dry_run
  rescue Dependabot::DependencyFileNotFound
    puts "ERROR: no files found in dir: #{dir}"
    exit(1)
  end
end

puts "  - Done"