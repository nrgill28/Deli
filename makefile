export SHELL            = /bin/bash

# Settings
export CONFIG          ?= Release
export FRAMEWORK       ?= net35
export VERSION         ?= 0.0.0

# DLL metadata
export GIT_DESCRIBE     = $(shell git describe --long --always --dirty)
export GIT_BRANCH       = $(shell git rev-parse --abbrev-ref HEAD)
export GIT_HASH         = $(shell git rev-parse HEAD)
export BUILD_PROPERTIES = /p:Version="$(VERSION)" /p:RepositoryBranch="$(GIT_BRANCH)" /p:RepositoryCommit="$(GIT_HASH)"

# Local
PROJECTS = Deli Deli.Core

.PHONY: all

all: $(PROJECTS)
	for p in $^; do \
		$(MAKE) -C $$p NAME=$$p; \
	done