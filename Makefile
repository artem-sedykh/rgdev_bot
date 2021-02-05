#SHELL := /bin/bash

ARGS = `arg="$(filter-out $@,$(MAKECMDGOALS))" && echo $${arg:-${1}}`

update_secrets_sample:
	@echo "Masking env files..."
	@find . -name *.env | xargs -I{} cp {} {}.sample
	@find . -name *.env.sample | xargs -I{} sed -i "s/\=.*/\=xxxxxxxxx/g" {}

#	@cat zigbee2mqtt/settings/configuration.yaml | sed "s/\:.*/\: xxxxxxxxx/g" > zigbee2mqtt/settings/configuration.yaml.sample #mask passwords

commit: update_secrets_sample
	git add .
	git diff-index --quiet HEAD || git commit -m "$(call ARGS,\"updating configuration\")"
	git push

%:
    @:
