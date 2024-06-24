SHELL := /bin/bash
.SILENT:

test:
	docker-compose run --rm dotnet .buildkite/scripts/test.sh || make down

integration-test:
	docker-compose -f docker-compose-test.yml run --rm dotnet .buildkite/scripts/integration-test.sh || make down

down:
	docker-compose -f docker-compose-test.yml down

stryker:
	docker-compose run --rm dotnet .buildkite/scripts/stryker.sh || make down