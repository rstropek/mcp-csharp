# Introduction to Model Context Protocol (MCP) - C#/.NET

## Overview

This folder contains samples for an introduction to the Model Context Protocol (MCP) using C#/.NET.

Test the samples with the [MCP Inspector Tool](https://modelcontextprotocol.io/docs/tools/inspector):

```sh
npx @modelcontextprotocol/inspector
# Inside inspector, choose "STDIO" transport and run the desired sample with: `dotnet run --project <sample-folder>`
```

Alternatively, you can use any other MCP client (e.g. VSCode, Cursor, Claude Desktop, etc.). VSCode is recommended as it has one of the most complete MCP client implementations.

## Docs

* [Model Context Protocol (MCP)](https://modelcontextprotocol.io)
* [VSCode MCP Developer Guide](https://code.visualstudio.com/api/extension-guides/ai/mcp)
  * Compare to [Claude Desktop Remote MCP Server Guide](https://support.claude.com/en/articles/11503834-building-custom-connectors-via-remote-mcp-servers)
  * Compare to [ChatGPT Developer Mode](https://platform.openai.com/docs/guides/developer-mode) for full MCP client capabilities ([change developer mode](https://chatgpt.com/#settings/Connectors))


## Samples

### Sample 1: MCP Server Without SDK (`10-McpServerNoSdk`)

This sample demonstrates how to set up an MCP server without using the MCP SDK. It communicates with the MCP client using raw JSON-RPC messages. **Do not write MCP server like this in production!** This is just for educational purposes to show how the protocol works under the hood.

The MCP server can generate passwords by concatenating winter-themed words.

### Sample 2: MCP Server With SDK (`20-McpServerSdk`)

The second sample implements the same functionality as the first sample, but this time it uses the MCP SDK. This makes the implementation much simpler and more robust.

The sample contains two tools (`winter_password` for single password generation and `winter_password_batch` for generating multiple passwords), a prompt, and a resource.

### Sample 3: Simple MCP Client (`30-McpClient`)

This sample shows how to create an MCP client with _stdio_ transport. It queries the server for the list of tools and tests both the `winter_password` and `winter_password_batch` tools.

### Sample 4: MCP Server With Sampling (`40-McpServerSampling`)

This example introduces the concept of sampling in MCP. The server can generate passwords by sampling winter-related words from an LLM at runtime.

This sample should be tested with VSCode and sampling enabled. It can be tested with MCP Inspector, but the sampling feature is only simulated there.

### Sample 5: MCP Server With Sampling and Image Processing

This example shows how to work with content that is not text. It implements an MCP server that uses sampling to verify images.

This sample should be tested with VSCode and sampling enabled. It can be tested with MCP Inspector, but the sampling feature is only simulated there.

