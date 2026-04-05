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

## [Phase 2] — .NET 9 Project + Data Access Layer — ✅ Complete
**Date:** 27 March 2026

### What I built
- .NET 9 solution with three projects: Api, Core, Infrastructure
- Clean Architecture — Core has zero dependencies, everything depends on Core
- Entities: Document, DocumentChunk, IngestionJob
- Interfaces: IDocumentRepository, IDocumentChunkRepository,
  IIngestionJobRepository, IEmbeddingService
- DTOs: CreateDocumentRequest, DocumentResponse, QueryRequest, QueryResponse
- DbConnectionFactory using Npgsql
- Repositories: DocumentRepository, DocumentChunkRepository, IngestionJobRepository
- EmbeddingService — calls Ollama REST API, returns float[]
- Dependency injection wiring in Program.cs
- Swagger UI configured

### Why Dapper over EF Core
- pgvector's <=> cosine distance operator is not supported by EF Core
- Writing raw SQL inside EF defeats its purpose
- Dapper gives full SQL control with clean result mapping
- Consistent approach across all queries

### Learned
- Clean Architecture separates what (Core) from how (Infrastructure)
- Interfaces are contracts — the API never knows which database is behind them
- DTOs protect the API contract from database model changes
- Dependency injection means never calling new on a service directly

---

## [Phase 3] — Document Ingestion Pipeline — ✅ Complete
**Date:** 28 March 2026

### What I built
- DocumentChunker — sliding window with configurable size (500) and overlap (50)
- DocumentIngestionService — orchestrates chunk → embed → store pipeline
- POST /api/Documents endpoint returning 202 Accepted
- Verified: chunks and vector embeddings stored in PostgreSQL

### Learned
- Chunking splits documents so LLMs can focus on relevant content
- Overlap prevents important context being cut at chunk boundaries
- Each chunk gets its own 768-dimension embedding vector
- The ingestion job tracks status: pending → processing → completed/failed

---

## [Phase 4] — RabbitMQ Async Processing — ✅ Complete
**Date:** 29 March 2026

### What I built
- IngestionMessage model in Core/Models
- IngestionMessageProducer — publishes messages to RabbitMQ
- IngestionWorker — IHostedService background worker
- Updated controller to return 202 instantly, process in background
- Durable queue and persistent messages — survive restarts

### Learned
- Async processing decouples the API response from heavy processing
- HTTP 202 Accepted is the correct status for async operations
- BasicAck/BasicNack tells RabbitMQ whether to remove or requeue a message
- prefetchCount=1 ensures the worker processes one document at a time

---

## [Phase 5] — RAG Query Pipeline — ✅ Complete
**Date:** 31 March 2026

### What I built
- RagQueryService — full RAG pipeline in one service
- POST /api/Query endpoint
- Vector similarity search using pgvector HNSW index
- Source attribution — document title returned with every answer
- Real similarity scores using 1 - cosine distance formula

### Key fix
- IVFFlat index requires minimum ~100 vectors to work correctly
- Switched to HNSW index which works at any dataset size
- HNSW (Hierarchical Navigable Small World) is better for small-medium datasets

### Learned
- RAG = retrieve relevant chunks first, then generate answer from context
- Cosine distance 0 = identical, 1 = completely different
- Similarity score = 1 - distance, so higher = more relevant
- The LLM only reads the retrieved chunks — it cannot hallucinate beyond them
- Prompt engineering matters — "answer ONLY from context" prevents guessing

---

## [Phase 6] — Polish + Documentation — ✅ Complete
**Date:** 05 April 2026

### What I built
- Updated README with complete build status
- Full CHANGELOG documenting every phase
- Demo script with 4 SD Organization documents
- Similarity scores on all query responses