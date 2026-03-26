# Changelog

All notable changes to SmartAssist are documented here.
Each entry maps to a phase of the build.

---

## [Phase 1] — Infrastructure Setup — ✅ Complete
**Date:** 26 March 2026

### What I built
- `docker-compose.yml` with three services: PostgreSQL, RabbitMQ, Ollama
- `.env` file for all credentials and configuration
- `init-db/01-init.sql` — auto-runs on first Postgres start
  - `documents` table — stores file metadata
  - `document_chunks` table — stores text chunks + vector(768) embeddings
  - `ingestion_jobs` table — tracks async processing status
- IVFFlat index on `document_chunks.embedding` for fast cosine similarity search
- Health checks on Postgres and RabbitMQ containers
- Named Docker volumes for data persistence across restarts
- Pulled two Ollama models:
  - `nomic-embed-text` (274MB) — converts text to 768-dim vectors
  - `llama3.2` (2.0GB) — local language model for answer generation

### What I learned
- Docker containers are isolated mini-computers — each service runs
  independently with its own filesystem and network
- pgvector adds a native `vector(768)` column type to PostgreSQL,
  turning it into both a relational and vector database
- IVFFlat clusters vectors into lists (buckets) so similarity search
  scans only the nearest cluster — much faster than full table scan
- Embeddings = the vector output of converting text to numbers
- nomic-embed-text and llama3.2 serve different roles:
  one finds relevant content, the other generates the answer
- RAG is necessary because LLMs answer from memory and can be wrong —
  retrieval grounds the answer in your actual data

### Verified
- All three containers running and healthy
- PostgreSQL tables created automatically from init-db/01-init.sql
- RabbitMQ management UI accessible at http://localhost:15672
- Ollama API responding at http://localhost:11434
- llama3.2 generating responses
- nomic-embed-text ready for embedding requests

---

## [Phase 2] — .NET Project + Data Access Layer — 🔄 In progress

---

## [Phase 3] — Document Ingestion Pipeline — ⏳ Planned

---

## [Phase 4] — RabbitMQ Async Worker — ⏳ Planned

---

---

## [Phase 5] — RAG Query Pipeline — ⏳ Planned

---

## [Phase 6] — REST API + Integration — ⏳ Planned