
<p align="center">
  <img width="30%" height="30%" alt="Github" src="https://github.com/user-attachments/assets/39024e44-5e02-4454-b84d-aa356e02c554" />
</p>

# SnapEngine

SnapEngine is a modular C# game engine built on top of [SFML.Net](https://www.sfml-dev.org/download/sfml.net/), designed for fast prototyping and embedded resource handling. It is ideal for hobbyist and experimental game developers who want full control over the engine architecture with minimal boilerplate.

---

## 🚀 Features

- **Beacon Manager** — A powerful pub-sub system for event-driven programming. Easily connect, emit, and listen to signals across your game with built-in attribute support for automatic beacon registration.

- **Entity Framework** — Simplifies game object creation and management. Focus on game logic instead of boilerplate setup.

- **Built-in Texture Atlas Manager** — Automatically handles texture slots and atlasing, so you don’t have to worry about texture batching or limits.

- **Full LDTK Support** — Seamlessly load and integrate [LDTK](https://ldtk.io/) (Level Designer Toolkit) maps, making level design and world-building straightforward.

- **Tweens** — Smoothly animate values, positions, and properties with an easy-to-use tweening system.

- **Advanced Save System** — Robust game save manager supporting compression, encryption, checksums, versioning, and safe file paths to protect your player data.

- **Advanced Graphics Library** — High-performance rendering with rich support for entities like alignment helpers, render targets, text sprites, ninepatches, and more.

- **Comprehensive Input Support** — Powered by a detailed SDL database for precise and extensive input handling.

- **Coroutines** — Native coroutine support to write asynchronous game logic in a clean, linear style.

- **Advanced Logging** — Flexible logging system with sinks for customized output and diagnostics.

- **Integrated Packer (Snap.Pack)** — Build, compress, and encrypt asset bundles (`.spack files`) for fast loading and secure distribution. Supports Brotli/Deflate compression, AES-GCM encryption with AAD, size verification, and CLI tools for building, listing, and verifying packs.

---

## 📦 Installation

Install via NuGet:

```bash
dotnet add package SnapEngine
