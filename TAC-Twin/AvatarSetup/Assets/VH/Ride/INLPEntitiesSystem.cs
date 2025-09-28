using System;

namespace Ride.NLP
{
    /// <summary>
    /// Holds NLP Entities text analytics request to be sent, typically a block of user input text.
    /// </summary>
    public class NLPEntitiesRequest : NlpRequest
    {
        public string text;

        public NLPEntitiesRequest(string text) : base(text)
        {
            this.text = text;
        }
    }

    /// <summary>
    /// Holds NLP Entities text analytics response, including
    /// original text, category, subcategory, and confidence score.
    /// </summary>
    public class NLPEntitiesResponse : NlpResponse
    {
        public NLPEntity[] entities;

        public struct NLPEntity
        {
            public string text;
            public string category;
            public string subcategory;
            public double confidenceScore;
        }

        public NLPEntitiesResponse(string response) : base(response) { }

        public NLPEntitiesResponse(string response, NLPEntity[] entities) : base(response)
        {
            this.entities = entities;
        }
    }

    /// <summary>
    /// Interface for calling NLP Entities text analytics service.
    /// </summary>
    public interface INLPEntitiesSystem : INlpSystem
    {
        /// <summary>
        /// Analyses entities of a given text.
        /// </summary>
        /// <param name="text">Text input to be analyzed</param>
        /// <param name="onComplete">Delegate to execute on successful request</param>
        void AnalyzeEntities(string text, Action<NlpResponse> onComplete);
    }
}
