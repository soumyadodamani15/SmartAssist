# SmartAssist — Enterprise Knowledge Assistant

> Ask your company's documents a question. Get a precise, grounded answer. No hallucinations, no cloud APIs, no data leaving your infrastructure.

![Build Status](https://img.shields.io/badge/status-in%20progress-orange?style=flat-square)
![Phase](https://img.shields.io/badge/phase-2%20of%206-blue?style=flat-square)
![Stack](https://img.shields.io/badge/.NET-8.0-purple?style=flat-square)
![License](https://img.shields.io/badge/license-MIT-green?style=flat-square)

---

## What is this?

SmartAssist is a **production-grade RAG (Retrieval-Augmented Generation) pipeline** built as a .NET 8 microservice. It sits on top of a company's existing documents — PDFs, policies, FAQs, product manuals — and lets employees ask natural language questions, getting intelligent answers grounded in real company knowledge.

Instead of hallucinating, the system **retrieves** the most relevant document chunks first, then uses an LLM to reason over only that context. Every answer is traceable back to a source.

```
User: "What is our refund policy for enterprise customers?"
SmartAssist: "Enterprise customers are eligible for a full refund within 30 days...
              [Source: enterprise-policy-2024.pdf, page 4]"
```

---

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    SmartAssist System                    │
│                                                         │
│  ┌──────────┐    ┌─────────────┐    ┌───────────────┐  │
│  │  REST    │    │  Ingestion  │    │  RAG Query    │  │
│  │  API     │───▶│  Service    │    │  Pipeline     │  │
│  │ .NET 8   │    │  + Chunker  │    │  Semantic     │  │
│  └──────────┘    └──────┬──────┘    │  Kernel       │  │
│                         │           └───────┬───────┘  │
│                   ┌─────▼──────┐            │          │
│                   │  RabbitMQ  │            │          │
│                   │   Queue    │            │          │
│                   └─────┬──────┘            │          │
│                         │                   │          │
│               ┌─────────▼───────────────────▼──────┐  │
│               │         PostgreSQL + pgvector        │  │
│               │   documents │ chunks │ embeddings    │  │
│               └────────────────────────────────────┘  │
│                                                         │
│               ┌─────────────────────────────────────┐  │
│               │    Ollama (local LLM inference)      │  │
│               │  llama3.2 (chat) + nomic-embed-text  │  │
│               └─────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
```

---

## Tech Stack

| Layer | Technology | Why |
|---|---|---|
| API Framework | ASP.NET Core 8 | Production-grade, minimal APIs |
| AI Orchestration | Semantic Kernel | Microsoft's LLM SDK for .NET |
| Vector Database | PostgreSQL + pgvector | SQL + vectors in one place |
| Message Queue | RabbitMQ | Async doc ingestion at scale |
| Local LLM | Ollama + llama3.2 | Zero API cost, data stays local |
| Embeddings | nomic-embed-text | 768-dim, open source |
| Containerisation | Docker Compose | One-command local setup |

---

## Build Progress

| Phase | What | Status |
|---|---|---|
| Phase 1 | Docker infrastructure (Postgres, RabbitMQ, Ollama) | ✅ Complete |
| Phase 2 | .NET project + data access layer | 🔄 In progress |
| Phase 3 | Document ingestion pipeline + chunking | ⏳ Planned |
| Phase 4 | RabbitMQ async worker | ⏳ Planned |
| Phase 5 | RAG query pipeline | ⏳ Planned |
| Phase 6 | REST API + integration | ⏳ Planned |

---

## Key Concepts Implemented

- **RAG Pipeline** — retrieve before generating; answers grounded in real data
- **Vector similarity search** — cosine distance via pgvector IVFFlat index
- **Document chunking** — overlapping sliding window for context preservation
- **Async processing** — RabbitMQ decouples upload from CPU-intensive embedding
- **Local inference** — Ollama runs llama3.2 + nomic-embed-text with no external API calls

---

## Running Locally

**Prerequisites:** Docker Desktop, .NET 8 SDK

```bash
# Clone
git clone https://github.com/YOUR_USERNAME/SmartAssist.git
cd SmartAssist

# Start all infrastructure
docker compose up -d

# Verify services
curl http://localhost:11434/api/tags    # Ollama
open http://localhost:15672             # RabbitMQ UI (smartassist / smartassist_secret)

# Run the API (Phase 3+)
cd SmartAssist.Api
dotnet run
```

---

## Project Journal

I'm building this in public, one phase at a time. Each phase has a corresponding LinkedIn post explaining the decisions made, problems hit, and concepts learned.

| Date | Phase | Post |
|---|---|---|
| — | Phase 1: Docker + pgvector setup | _coming soon_ |
| — | Phase 2: .NET project structure + EF Core | _coming soon_ |

---

## Why I built this

I'm a full-stack developer transitioning into AI engineering. I wanted to understand RAG pipelines from the ground up — not by wrapping an OpenAI call, but by building the retrieval, embedding, and generation layers myself, on a production-grade .NET stack with real infrastructure.

Everything here is decisions I made and understood, not boilerplate I copied.

---

## Roadmap (post Phase 6)

- [ ] Hybrid search (BM25 + vector) for better recall
- [ ] Re-ranking layer (cross-encoder model)
- [ ] Multi-tenant document isolation
- [ ] Streaming responses via SSE
- [ ] React frontend with source highlighting

---

*Built with .NET 8 · Semantic Kernel · PostgreSQL · pgvector · RabbitMQ · Ollama*
