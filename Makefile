include ./Makefile.base.mk

# -- cosmetics --
help-column-width = 5

# -- init --
## initializes the dev environment
init: init/pre init/base
.PHONY: init

# -- init/helpers
init/base:
	brew bundle -v
.PHONY: init/base

init/pre:
ifeq ("$(shell command -v brew)", "")
	$(info ✘ brew is not installed, please see:)
	$(info - https://brew.sh)
	$(error 1)
endif
.PHONY: init/pre

# -- test --
## alias for t/lib
test: t/lib
.PHONY: test

## runs the tests for the library target
t/lib:
	dotnet test
.PHONY: t/lib
