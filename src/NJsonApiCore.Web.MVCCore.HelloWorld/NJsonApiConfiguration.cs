using NJsonApi.Serialization.Representations;
using NJsonApi.Web.MVCCore.HelloWorld.Controllers;
using NJsonApi.Web.MVCCore.HelloWorld.Models;
using System.Collections.Generic;

namespace NJsonApi.Web.MVCCore.HelloWorld
{
    public static class NJsonApiConfiguration
    {
        public static IConfiguration BuildConfiguration()
        {
            var configBuilder = new ConfigurationBuilder();

            var articleMeta = new Dictionary<string, object>();
            articleMeta.Add("Allowed", "Of course papa!");
            articleMeta.Add("method", "POST");

            configBuilder
                .Resource<Article, ArticlesController>()
                .WithAllProperties()
                .WithTopLevelLink(new ObjectLink("AddCart", articleMeta, new System.Uri("http://www.example.com/api/v2/articles/actions/addToCart")));

            configBuilder
                .Resource<Person, PeopleController>()
                .WithAllProperties();

            configBuilder
                .Resource<Comment, CommentsController>()
                .WithAllProperties();

            var nJsonApiConfig = configBuilder.Build();
            return nJsonApiConfig;
        }
    }
}