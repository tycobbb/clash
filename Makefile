include ./Makefile.base.mk

# -- cosmetics --
help-column-width = 5

# -- context --
lib_name     = Clash
lib_root     = $(lib_name)
lib_build    = $(lib_root)/bin/Debug/netstandard2.0
lib_dll_name = $(lib_name).dll
lib_pdb_name = $(lib_name).pdb
lib_dll      = $(lib_build)/$(lib_dll_name)
lib_pdb      = $(lib_build)/$(lib_pdb_name)

app_name     = $(lib_name).Unity
app_root     = $(app_name)
app_libs     = $(app_root)/Assets/Plugins
app_lib_dll  = $(app_libs)/$(lib_dll_name)
app_lib_pdb  = $(app_libs)/$(lib_pdb_name)

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
b/lib: b/lib/compile b/lib/install
.PHONY: b/lib

b/lib/compile:
	dotnet build $(lib_root)
	mkdir -p $(app_libs)
.PHONY: b/lib/compile

b/lib/install: $(app_lib_dll) $(app_lib_pdb)
.PHONY: b/lib/install

$(app_libs):
	mkdir -p $(app_libs)

$(app_lib_dll): $(app_libs)
	ln -s $(CURDIR)/$(lib_dll) $(CURDIR)/$(app_libs)

$(app_lib_pdb): $(app_libs)
	ln -s $(CURDIR)/$(lib_pdb) $(CURDIR)/$(app_libs)

# -- test --
## alias for t/lib
test: t/lib
.PHONY: test

## runs the tests for the library target
t/lib:
	dotnet test
.PHONY: t/lib
