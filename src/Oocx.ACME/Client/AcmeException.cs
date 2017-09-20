using System;
using System.Net.Http;
using Oocx.Acme.Protocol;

namespace Oocx.Acme.Client
{
    public class AcmeException : Exception
    {
        public Problem Problem { get; }
        public HttpResponseMessage Response { get; }

        public AcmeException(Problem problem, HttpResponseMessage response)
            : base($"{problem.Type}: {problem.Detail}")
        {
            Problem = problem;
            Response = response;
        }
    }
}