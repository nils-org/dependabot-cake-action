#!/usr/bin/env bash
set -e
pushd /usr/src/app > /dev/null
bundle exec ./dependabot.rb
popd