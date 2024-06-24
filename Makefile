SHELL := /bin/bash
.SILENT:

test:
	docker-compose run --rm dotnet .buildkite/scripts/test.sh

integration-test:
	docker-compose -f docker-compose-test.yml run --rm dotnet .buildkite/scripts/integration-test.sh 
stryker:
	docker-compose run --rm dotnet .buildkite/scripts/stryker.sh
sonarqube:
	docker-compose run --rm sonarqube .buildkite/scripts/sonarqube.sh

down:
	docker-compose -f docker-compose-test.yml down