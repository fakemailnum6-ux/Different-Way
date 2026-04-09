using Godot;
using System.Collections.Generic;

namespace DifferentWay.Database
{
    public partial class VectorDB : Node
    {
        // In Phase 2, this acts as a stub interface for future RAG implementation
        // (Retrieval-Augmented Generation) using something like sqlite-vss.

        public override void _Ready()
        {
        }

        public void StoreMemory(string npcId, string text)
        {
             // Stub: would calculate embeddings and store in DB
             GD.Print($"[VectorDB] Stored memory for {npcId}: {text}");
        }

        public List<string> RetrieveRelevantMemories(string npcId, string queryText)
        {
             // Stub: would perform cosine similarity search
             // Returning empty for now so we default to FIFO context only
             return new List<string>();
        }
    }
}
