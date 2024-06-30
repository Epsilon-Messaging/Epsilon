SHELL := /bin/bash
.SILENT:

test:
	docker-compose -f .buildkite/docker-compose.yml run --rm dotnet .buildkite/scripts/test.sh

integration-test:
	(docker-compose -f .buildkite/docker-compose-test.yml run --rm dotnet .buildkite/scripts/integration-test.sh || make send_logs) && make down 

stryker:
	docker-compose -f .buildkite/docker-compose.yml run --rm dotnet .buildkite/scripts/stryker.sh

sonarqube:
	docker-compose -f .buildkite/docker-compose.yml run --rm sonarqube .buildkite/scripts/sonarqube.sh

publish:
	docker-compose -f .buildkite/docker-compose.yml run --rm dotnet .buildkite/scripts/publish.sh

deploy:
	.buildkite/scripts/deploy.sh

down:
	echo "Down"
	docker-compose -f docker-compose.yml -f .buildkite/docker-compose-test.yml down
	
send_logs:
	.buildkite/scripts/docker-logs.sh
	make down
	exit 1