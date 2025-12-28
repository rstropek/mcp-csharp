# WinterPasswordLib

A C# library for generating secure, memorable passwords using winter-themed words.

## Overview

WinterPasswordLib provides functionality to generate passwords by concatenating randomly selected winter-themed words (primarily German winter vocabulary). The generated passwords are both memorable and secure, with optional special character substitutions to meet common password complexity requirements.

## Features

- **Random Password Generation**: Builds passwords by concatenating random winter words until a minimum length is reached
- **Customizable Length**: Configure minimum password length via `PasswordGenerationOptions`
- **Special Character Substitution**: Optional character substitutions for enhanced security:
  - `o/O` → `0`
  - `i/I` → `!`
  - `e/E` → `€`
  - `s/S` → `$`
- **Batch Generation**: Generate multiple passwords at once using `BuildMany()`
- **Custom Word Lists**: Use your own word list or the built-in default German winter words

## Usage

### Basic Password Generation

```csharp
using WinterPasswordLib;

// Generate a simple password with default settings (min 16 characters)
var password = PasswordGenerator.BuildPassword(new PasswordGenerationOptions());
// Example output: "SchneeWinterFrost"
```

### Password with Special Characters

```csharp
// Generate a password with special character substitutions
var options = new PasswordGenerationOptions(MinLength: 20, Special: true);
var password = PasswordGenerator.BuildPassword(options);
// Example output: "$chn€€W!nt€rFr0$t€!$"
```

### Generate Multiple Passwords

```csharp
// Generate 5 passwords at once
var passwords = PasswordGenerator.BuildMany(
    count: 5,
    opts: new PasswordGenerationOptions(MinLength: 16, Special: true)
);
```

### Using Custom Word Lists

```csharp
string[] customWords = ["Snow", "Ice", "Cold", "Frost", "Winter"];
var password = PasswordGenerator.BuildPassword(
    new PasswordGenerationOptions(MinLength: 12),
    words: customWords
);
```

## Configuration Options

### PasswordGenerationOptions

- `MinLength` (int, default: 16): Minimum length of the generated password
- `Special` (bool, default: false): Enable special character substitutions

## Default Word List

The library includes 40 German winter-themed words including:
- Schnee (snow), Eis (ice), Frost (frost)
- Schneeflocke (snowflake), Schneemann (snowman)
- Wintertag (winter day), Winterwald (winter forest)
- And many more winter-related terms
