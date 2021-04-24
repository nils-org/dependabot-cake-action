FROM ruby:2.6.6-alpine

LABEL "maintainer"="Nils Andresen <nils@nils-andresen.de>" \
      "repository"="https://github.com/nils-org/dependabot-cake-action" \
      "homepage"="https://github.com/nils-org/dependabot-cake-action" \
      "com.github.actions.name"="dependabot for cake" \
      "com.github.actions.description"="Run dependabot for cake as github action" \
      "com.github.actions.icon"="check-circle" \
      "com.github.actions.color"="package"

WORKDIR /usr/src/app

COPY ./app /usr/src/app
COPY ./run-action.sh /usr/local/bin/run-action.sh

# install needed tools
RUN apk add --no-cache git build-base

# install rb dependencies & remove intermediate files
RUN bundle install \
    # Remove unneeded files (cached *.gem, *.o, *.c)
    && rm -rf /usr/local/lib/ruby/gems/2.6.0/cache/*.gem \
    && find /usr/local/lib/ruby/gems/2.6.0/ -name "*.c" -delete \
    && find /usr/local/lib/ruby/gems/2.6.0/ -name "*.o" -delete

# remove runtime-unnedded tools & cleanup
RUN apk del build-base && \
    rm -rf /var/lib/apt/lists/* /tmp/* /var/tmp/*

CMD ["run-action.sh"]