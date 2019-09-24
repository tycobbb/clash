include ./Makefile.base.mk

# -- cosmetics --
help-column-width = 5

# -- context --
lib_name  = Clash
lib_root  = ./$(lib_name)
lib_build = $(lib_root)/bin/Debug/netstandard2.0
lib_dll   = $(lib_build)/$(lib_name).dll
app_name  = $(lib_name).Unity
app_root  = ./$(app_name)
app_libs  = $(app_root)/Assets/Plugins

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

# -- build --
## alias for b/lib
build: b/lib
.PHONY: build

## builds and installs the library target
b/lib:
	dotnet build $(lib_root)
	mkdir -p $(app_libs)
	cp $(lib_dll) $(app_libs)
.PHONY: b/lib

# -- test --
## alias for t/lib
test: t/lib
.PHONY: test

## runs the tests for the library target
t/lib:
	dotnet test
.PHONY: t/lib
