# Advanced MCP with .NET Aspire - C#/.NET

## Overview

This repository contains samples demonstrating advanced Model Context Protocol (MCP) concepts using C#/.NET with Aspire orchestration. These samples build upon the foundational MCP concepts from [10-Basics](../10-Basics/), showcasing HTTP transport, streamable responses, and modern cloud-native application patterns.

You can start the Aspire app host with `dotnet run --project AppHost`. Next, test the samples with the [MCP Inspector Tool](https://modelcontextprotocol.io/docs/tools/inspector).

## Infrastructure

### Aspire App Host (`AppHost`)

The `AppHost` project serves as the orchestrator for all services in this solution. It uses Aspire to manage service discovery, health checks, and telemetry across the distributed application.

### Service Defaults (`ServiceDefaults`)

A shared library providing common Aspire services.

## Samples

### Sample 1: Demo Server (`10-DemoServer`)

A simple HTTP-based MCP server that demonstrates basic MCP tool capabilities with HTTP transport. This server provides an `echo-tool` that echoes back messages and optionally simulates processing time.

### Sample 2: MCP Streamable Server (`20-McpStreamableServer`)

An advanced MCP server that showcases sophisticated features.

### Sample 3: MCP Streamable Server with Authentication (`30-McpStreamableAuth`)

An MCP server that integrates authentication and authorization using JWT Bearer tokens, demonstrating secure access to MCP.
