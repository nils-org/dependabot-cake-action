# dependabot-cake-action

A github action for running dependabot on repositories using cake-build.

## Table of Contents

- [Background](#background)
- [Install](#install)
- [Usage](#usage)
- [Limitations](#limitations)
- [Idea / Attribution](#idea--attribution)
- [Full Example](#full-example)
- [Running Locally](#running-locally)
- [Maintainers](#maintainers)
- [Contributing](#contributing)
- [License](#license)

## Background

This action provides the features, as developed for https://github.com/dependabot/dependabot-core/pull/1848 (a PR for https://github.com/dependabot/dependabot-core/issues/733): **To have dependabot check cake-references**.

Currently dependabot does not support this and sadly merging https://github.com/dependabot/dependabot-core/pull/1848 might take some time. In the meantime it is possible to use the code provided in the PR to do the checking "manually".

This action provides the means to do so.

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
```

## Limitations

This not a real dependabot, so there so "commands" to give (like `@dependabot rebase` and such). If you need to rebase the PR, you'll have to do that manually.

TODOs:
*  Check what happens when a PR is not merged and closed. Will it simply be re-created every run?
* How to "ignore" dependencies? 

## Idea / Attribution

Most of this was shamelessly copied from https://github.com/patrickjahns/dependabot-terraform-action/

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
  * `--test-no-dryrun=true` if set, real PRs are created.

## Maintainers

[Nils Andresen @nils-a](https://github.com/nils-a).

## Contributing
We accept Pull Requests.

Small note: If editing the Readme, please conform to the [standard-readme](https://github.com/RichardLitt/standard-readme) specification. 

### Contributors

* Nils Andresen

## License

[MIT License © Nils Andresen](LICENSE.txt)