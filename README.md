# NSwagen

NSwagen is a pluggable command line tool to generate the code from swagger definitions.

## Getting Started
This section discusses installing the nswagen tool globally.

```sh
# Install NSwagen CLI
dotnet tool install --global dotnet-nswagen

# Update NSwagen CLI
dotnet tool update --global dotnet-nswagen
```
## Usage

```sh
# Lists all the available commands
nswagen help

# Create nswagen configuration file
# This will create a file called nswagen.config.json under current directory
nswagen config init [option]

# It adds the additional configuration for the existing [nswagen.config.json] or specified configuration file.
nswagen config add [option]

# List the generator information
nswagen info [option]

# Creates a single client code.
nswagen create [option]

# Generate multiple client code.
nswagen generate [option]
```
## Documentation
NSwagen documentation can be found in the [wiki](https://github.com/prathimanm/NSwagen/wiki/NSwagen-documentation)


