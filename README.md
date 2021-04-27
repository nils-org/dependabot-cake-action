# dependabot-cake-action

A github action for running dependabot on repositories using cake-build.

## Table of Contents

- [Goal](#goal)  
- [Install](#install)
- [Usage](#usage)
- [Limitations](#limitations)
- [Full Example](#full-example)
- [Background](#background)
- [Idea / Attribution](#idea--attribution)
- [Running Locally](#running-locally)
- [Alternatives](#alternatives) 
- [Maintainers](#maintainers)
- [Contributing](#contributing)
- [License](#license)

## Goal

The goal of this project is two-fold:
- Enable users of dependabot to have a dependabot-like way to keep Cake dependencies up-to-date.
- To keep the code for integrating Cake as a new ecosystem in dependabot up-to-date and tested.

To that end, I have forked the original PR into a [custom repo](https://github.com/nils-org/dependabot-core/tree/cake/main) 
where I try to keep the original code from [dependabot-PR 1848](https://github.com/dependabot/dependabot-core/pull/1848) up-to-date
and error-free.

## Install

Use the action in your workflow yaml by adding a step with `uses: nils-org/dependabot-cake-action@v1`.

## Usage

```yml
- name: check/update cake references
  uses: nils-org/dependabot-cake-action@v1
  with:
    # Where to look for cake files to check for dependency upgrades.
    # The directory is relative to the repository's root.
    # Multiple paths can be provided by splitting them with a new line.
    # Example:
    #   directory: |
    #     /path/to/first/module
    #     /path/to/second/module
    # Default: "/"
    directory: ""

    # Branch to create pull requests against.
    # By default your repository's default branch is used.
    target_branch: ""

    # Auth token used to push the changes back to github and create the pull request with.
    # [Learn more about creating and using encrypted secrets](https://help.github.com/en/actions/automating-your-workflow-with-github-actions/creating-and-using-encrypted-secrets)
    # default: ${{ github.token }}
    token: ""

    # List of dependencies that will not be updated
    # Example:
    #   ignore: |
    #    Cake.7zip
    #    Cake.asciidoctorj
    # default: none
    ignore: ""
```

## Limitations

This not a real dependabot, so there are no "commands" to give (like `@dependabot rebase` and such). If you need to rebase the PR, you'll have to do that manually.

## Full Example
Save the following content in you're repo under `.github/workflows/dependabot-cake.yml`
```yml
name: dependabot-cake
on:
  workflow_dispatch:
  schedule:
    # run everyday at 6
    - cron:  '0 6 * * *'

jobs:
  dependabot-cake:
    runs-on: ubuntu-latest # linux, because this is a docker-action
    steps:
      - name: check/update cake dependencies
        uses: nils-org/dependabot-cake-action@v1
```

## Background

The original code was developed for https://github.com/dependabot/dependabot-core/pull/1848 (a PR for https://github.com/dependabot/dependabot-core/issues/733): **To have dependabot check cake-references**.

Currently dependabot has postponed adding new ecosystems and sadly merging https://github.com/dependabot/dependabot-core/pull/1848 might take some time.

## Idea / Attribution

Most of this was shamelessly copied from https://github.com/patrickjahns/dependabot-terraform-action/

## Running Locally
It is also possible to run this action locally:

* Clone this repo
* build the docker image

  `cd src && docker build -t dependabot-cake:develop .`
* run the container and give the needed environment-vars

  `docker run --rm -e DRY_RUN=1 -e GITHUB_REPOSITORY=nils-a/Cake.7zip -e INPUT_TARGET_BRANCH=develop -e INPUT_TOKEN=your-github-api-token dependabot-cake:develop`

## Cake targets

* `Build-Image` Creates the image.
  * `imageName=some-image-name` to set the image name. Default: `dependabot-cake`
* `Run-Test` Runs a container off the image locally. Settings:
  * `--test-RepositoryName=owner/repo` to set a repository. Default: `nils-a/Cake.7zip`
  * `--test-RepositoryBranch=branch` to set a branch. Default: `develop`
  * Environment variable `INPUT_TOKEN` must be set to a personal access token.
  * `--test-folder=subfolder` to set a folder to search. Can be given multiple times. Default: `["/"]`
  * `--test-no-dryrun` if set, real PRs are created.
  * `--test-ignore=Cake.7zip` ignore a dependency. Can be given multiple times. Default: `[]`

## Alternatives

One alternative to dependabot is [Renovate](https://www.whitesourcesoftware.com/free-developer-tools/renovate/)
which fully supports Cake. See the [post on cakebuild.net](https://cakebuild.net/blog/2021/04/cake-support-in-renovate) for a sample integration.

## Maintainers

[Nils Andresen @nils-a](https://github.com/nils-a).

## Contributing
We accept Pull Requests.

Small note: If editing the Readme, please conform to the [standard-readme](https://github.com/RichardLitt/standard-readme) specification. 

### Contributors

* Nils Andresen

## License

[MIT License © Nils Andresen](LICENSE.txt)