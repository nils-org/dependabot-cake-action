FROM ruby:2.6.6-slim

LABEL "maintainer"="Nils Andresen <nils@nils-andresen.de>" \
      "repository"="https://github.com/nils-org/dependabot-cake-action" \
      "homepage"="https://github.com/nils-org/dependabot-cake-action" \
      "com.github.actions.name"="dependabot-cake-action" \
      "com.github.actions.description"="Run dependabot for cake as github action" \
      "com.github.actions.icon"="check-circle" \
      "com.github.actions.color"="package"

WORKDIR /usr/src/app
#ENV DEPENDABOT_NATIVE_HELPERS_PATH="/usr/src/app/native-helpers"

COPY ./app /usr/src/app
COPY ./run-action.sh /usr/local/bin/run-action.sh

# install needed tools
RUN apt-get update && \
    apt-get install -y build-essential git wget

# install rb dependencies
RUN bundle install

# remove runtime-unnedded tools & cleanup
RUN apt-get remove -y build-essential && \
    apt-get autoremove -y && \
    apt-get clean && \
    rm -rf /tmp/*

CMD ["run-action.sh"]