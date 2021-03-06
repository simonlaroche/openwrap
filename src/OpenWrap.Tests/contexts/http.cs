using System;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Xml.Linq;
using OpenRasta.Client;
using OpenRasta.Client.Memory;
using OpenWrap.Testing;
using Tests.Repositories.factories;

namespace Tests.contexts
{
    public abstract class http : context
    {
        protected MemoryHttpClient Client = new MemoryHttpClient();

        protected void given_remote_resource(string uri, string mediaType, XDocument content, string username = null, string password = null)
        {
            given_remote_resource(uri, mediaType, content.ToString(), username, password);

        }
        protected void given_remote_resource(string uri, string mediaType, string content, string username = null, string password = null)
        {
            Client.Resources[uri.ToUri()] = new MemoryResource(new MediaType(mediaType), new MemoryStream(Encoding.UTF8.GetBytes(content)))
            {
                Username = username,
                Password = password
            };
        }

        protected void given_remote_resource(string uri, MemoryResource resource)
        {
            Client.Resources[uri.ToUri()] = resource;
        }

        protected void given_remote_resource(string uri,
                                             Expression<Func<IClientRequest, IClientResponse>> handler)
        {
            Client.Resources[uri.ToUri()] = new MemoryResource
            {
                Operations =
                    {
                        { handler.Parameters[0].Name, handler.Compile() }
                    }
            };
        }

        protected void given_not_found_response(Func<IClientRequest, bool> predicate, XDocument feed)
        {
            var existingResponse = Client.NotFoundResponse;
            Client.NotFoundResponse = request => !predicate(request)
                ? existingResponse(request) 
                : new MemoryResponse(200)
            {
                Entity = new MemoryEntity(new MediaType("application/atom+xml"), 
                    new MemoryStream(Encoding.UTF8.GetBytes(feed.ToString())))
            };
        }
    }
}