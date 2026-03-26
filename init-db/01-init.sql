-- Enable the vector extension
CREATE EXTENSION IF NOT EXISTS vector;

-- documents table
-- Stores metadata about each uploaded file
-- The actual text content lives in document_chunks
CREATE TABLE IF NOT EXISTS documents (
    id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    title        TEXT NOT NULL,
    source       TEXT NOT NULL,
    content_type TEXT NOT NULL,
    created_at   TIMESTAMPTZ DEFAULT NOW(),
    updated_at   TIMESTAMPTZ DEFAULT NOW()
);

-- document_chunks table
-- One row per chunk of text from a document
-- embedding column stores the 768-dimension vector
CREATE TABLE IF NOT EXISTS document_chunks (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    document_id UUID NOT NULL REFERENCES documents(id) ON DELETE CASCADE,
    chunk_index INTEGER NOT NULL,
    content     TEXT NOT NULL,
    embedding   vector(768),
    token_count INTEGER,
    created_at  TIMESTAMPTZ DEFAULT NOW()
);

-- ingestion_jobs table
-- Tracks async processing status for each document
-- Status flow: pending → processing → completed (or failed)
CREATE TABLE IF NOT EXISTS ingestion_jobs (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    document_id UUID REFERENCES documents(id) ON DELETE CASCADE,
    status      TEXT NOT NULL DEFAULT 'pending',
    error_msg   TEXT,
    created_at  TIMESTAMPTZ DEFAULT NOW(),
    updated_at  TIMESTAMPTZ DEFAULT NOW()
);

-- Vector similarity search index
-- IVFFlat divides vectors into 100 clusters (lists)
-- A search only scans the nearest cluster instead of every row
-- Much faster than scanning all rows for large datasets
CREATE INDEX IF NOT EXISTS idx_chunks_embedding
    ON document_chunks
    USING ivfflat (embedding vector_cosine_ops)
    WITH (lists = 100);

-- Regular index for filtering chunks by document
CREATE INDEX IF NOT EXISTS idx_chunks_document_id
    ON document_chunks(document_id);
