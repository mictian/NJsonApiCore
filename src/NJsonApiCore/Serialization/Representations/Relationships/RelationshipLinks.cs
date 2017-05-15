using Newtonsoft.Json;
using NJsonApiCore.Serialization.Representations.Resources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NJsonApi.Serialization.Representations.Relationships
{
    public class RelationshipLinks : LinkCollection
    {
        //[JsonProperty(PropertyName = "self", NullValueHandling = NullValueHandling.Ignore)]
        //public ILink Self { get; set; }

        //[JsonProperty(PropertyName = "related", NullValueHandling = NullValueHandling.Ignore)]
        public ILink Related
        {
            get
            {
                return Links.ContainsKey("related") ? Links["related"] : null;
            }
            set
            {
                this.AddOnce(value);
            }
        }

        public ILink Self
        {
            get
            {
                return Links.ContainsKey("self") ? Links["self"] : null;
            }
            set
            {
                this.AddOnce(value);
            }
        }

        new private void Add(ILink link) => base.Add(link);

        new private void AddOnce(ILink link) => base.AddOnce(link);

    }
}