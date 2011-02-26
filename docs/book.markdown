# C# Web API Programming with Hammock
_by Daniel Crenna_

#### Welcome to the Programmable Web

If you haven’t had the requirement to connect with another web service in your projects, you soon will. 
In an increasingly connected world, where the presence of an Application Programming Interface (API) is 
as essential to a growing company’s online success as a blog was five years ago, it’s inevitable that you 
will need to build software that integrates with multiple sources of data from numerous partners. 
This is achieved through API programming, and by way of adoption this is most often accomplished through
HTTP programming, based on the REST architectural style (to varying degrees), which provides convenient 
metaphors for creating, retrieving, updating, and deleting data over HTTP, as well as providing smooth 
transitions from one set of data to the next through the URIs embedded in a literal web of results. 

Notable companies that provide a REST-like API for their customers include Twitter, Facebook, Google, Amazon, 
and Microsoft. There are too many more to mention. When you are tasked with consuming multiple APIs, you need a 
simple, expressive toolset that allows you to rapidly build client code that is easy to understand, while 
allowing you to meet the needs of multiple styles, authentication requirements, and messaging. 
This is where Hammock comes in.

#### The Philosophy and Purpose of Hammock

Hammock's philosophy is simple: _REST, easy._
Hammock's purpose is to provide a common foundation for RESTful communication in .NET that gives developers a 
clean and easy way to build RESTful client libraries, or consume RESTful services from their applications, 
without compromising time or quality. It shares similar aspirations with other web API utilities, but focuses on 
completeness, performance, and getting out of your way. 

#### A Brief History of Hammock

Hammock grew from the networking code in TweetSharp (http://tweetsharp.com), .NET’s most popular Twitter API library. 
When the library’s authors found themselves maintaining numerous API projects, they turned to TweetSharp’s foundation again 
and again, pulling in snippets and adding new features. As APIs continued to explode online, it made sense to consolidate 
the code into a common library, add an abstraction layer to sweeten its use, and open it up to the public to benefit. 
The project was launched feature complete in April 2010 and is still in active development.

#### What Does It Do?

Hammock provides classes to help you consume RESTful services or develop your own client API libraries with a 
minimum of effort. It supports asynchronous operation, query caching and mocking, periodic and rate limited tasks,
object model serialization, multi-part forms, Basic and OAuth authentication, and timeout and retry policies. 
It is designed to allow you to extend it easily, so you can provide your own serialization scheme, custom validation, 
rate limiting rules, and web credentials. It also helps you unify projects across a wide range of platforms and 
devices, as it works on .NET, .NET Compact Framework, Silverlight, Windows Phone, Mono, MonoTouch, and MonoDroid.



## What You Need to Get Started
[]



## Creating REST Requests

This section covers creating a RESTful request issued from your application to the service you intend to consume.
Building requests in Hammock uses the language of HTTP, to make it easier to find the methods you're looking for.
If you need to add an HTTP header to your outgoing HttpRequest, you'd use the AddHeader method, and so on.

### RestClient, RestRequest, and RestResponse
These three classes form the backbone of Hammock, and provide all of the properties you need to build RESTful 
requests and process their results. RestClient and RestRequest have a special relationship; any property that can
be set on both classes obeys a hierarchical relationship, so you are free to set common values on RestClient, 
and override them with RestRequest, depending on your needs. This makes it easy and efficient to create a 
RestClient with the most common request values, and then use specific request values for each request sent through 
the client. An obvious example of that could be credentials; if your authorization requirements never change between
requests for the client you are consuming, then setting credentials on RestClient makes the most sense, and will allow
you to omit setting them on every RestRequest. 

As a working example, here is a simple Twitter API RestClient that stores basic information about the service, and a 
RestRequest that sets values, like credentials specific to the user, and the target API path, using your application.

[code:c#]
BasicAuthCredentials credentials = new BasicAuthCredentials
                                      {
                                          Username = "username",
                                          Password = "password"
                                      };

RestClient client = new RestClient
             {
                 Authority = "http://api.twitter.com",
                 VersionPath = "1"
             };

RestRequest request = new RestRequest
              {
                  Credentials = credentials,
                  Path = "statuses/home_timeline.json"
              };
              
RestResponse response = client.Request(request);
[/code:c#]

### Building Request Paths

The final URL for a request path is constructed first from the RestClient’s Authority property. From there, either RestRequest 
or RestClient is inspected for the VersionPath, and Path values. The VersionPath is always inserted between the Authority and 
Path values. It is useful for targeting specific versions of an API endpoint, or for any URI path fragment that varies between 
the authority and the target path. Hammock can handle secure endpoints, just ensure you include the HTTPS scheme when declaring 
your authority.

// TODO

Once your path is defined, you still need to add request state. State in a REST request refers to the query string parameters, 
POST parameters or fields, request headers, or entity body contents that make up the data sent to the specified path. 
In Hammock, you can do this one of two ways: using a traditional method API, or applying attributes to a custom class that is passed 
to Hammock.

### Request State Using Method Style

Properties and methods on both RestClient and RestRequest form the basis of most requests in Hammock: WebMethod, UserAgent, AddHeader, 
AddParameter, AddFile, AddField, and Entity form the list of major members. Methods that begin with Add allow setting multiple values, 
while the properties allow setting a single value. All of these members are allowable on either class, making it easier to build up 
clients with default behavior. The UserAgent property is a formalized way of setting the User-Agent request property, but it can be 
set using either the property or the AddHeader method. If both are explicitly set...

[]

The Entity property allows you to set the body of a POST or PUT request, and it can be set to any object. If you do not specify a 
WebMethod before setting the Entity, Hammock will assume you are making a POST request.

[]

### Request State Using Attribute Style

Some APIs, like TweetSharp, need a more stateful approach to assembling REST requests. For example, you may want to collect information 
about a particular request over time in your own class, and at request time, transform it into the appropriate header, parameter, 
user agent, and entity body values. You can certainly do this using the previous method style, but it would result in a lot of 
"left right" coding, or mapping state from your code to the most appropriate RestClient or RestRequest properties. To avoid having to 
perform this work, and to enable some advanced validation and transforming capabilities, you can use Hammock’s attributes style.

[]

### Using IWebQueryInfo

The IWebQueryInfo interface is an empty marker interface you can attach to the class you intend to pass to Hammock to build requests. 
Once your class implements IWebQueryInfo, you can use the built-in specialized attributes to declare essential request data like 
parameters, headers, user agent, and entity, and you can use ours or your own validation attributes to transform values or throw 
exceptions based on property values at the time of the request. Attributes are only applied to public properties defined in your class. 
Once you have an instance of your IWebQueryInfo class with attributes applied, you can set the Info property of RestClient or 
RestRequest to that instance, and Hammock will set all request values appropriately. As always, the Info set on RestRequest will 
override the Info set on RestClient.

[]

### Specialized Attributes

Specialized attributes set fundamental HTTP API elements on outgoing requests. By marking a public property with UserAgentAttribute, 
its value is used as the request’s "User-Agent" header value. ParameterAttribute and HeaderAttribute are used to set request URL or 
POST parameters and headers respectively, and require a name string to identify the name of the name-value pair to use. The next 
listing demonstrates an IWebQueryInfo class with some fundamental request values using attributes.

[]

### Validation Attributes

Hammock includes a number of validation attributes for your use, that we’ve found are useful for building libraries. The purpose of a 
validation attribute is to transform, at request time, the value of the decorated public property. This gives you an opportunity to 
course correct for your needs (such as is the case with DateFormatAttribute and BooleanToIntegerAttribute), or prevent issuing requests 
that don’t meet specific criteria (as in SpecificationAttribute and RequiredAttribute). You can also derive from ValidationAttribute 
to create your own custom validation or transformations. The following example demonstrates an IWebQueryInfo class with typical 
validation attributes, including the use of a Hammock-provided ValidEmailSpecification specification and SpecificationAttribute. 
In the example, a ValidationException will throw if the value of Email is not a valid email address at the time of a request, and if 
it were, the value of the valid email address would be passed in a parameter named "Contact" (either in the URL with GET or in the POST 
body, depending on the request).

[code:c#]
// A IWebQueryInfo implementation for custom validation
public class MyCustomInfo : IWebQueryInfo
{
    [Parameter("Contact")]
    [Specification(typeof(ValidEmailSpecification))]
    public string Email { get; set; }
}

// Example code that would throw an exception due to an invalid email address
IWebQueryInfo info = new MyCustomInfo { Email = "nowhere" };

RestClient client = new RestClient
{
    Authority = "http://nowhere.com",
    Info = info
};

RestRequest request = new RestRequest
{
    Path = "fast"
};

client.Request(request);

// From Hammock.Validation namespace, for illustration purposes
public class ValidEmailSpecification : HammockSpecification<string>
{
    // Accepts names, i.e. John Smith <john@johnsmith.com>
    private static readonly Regex _names =
        new Regex(
            @"\w*<([-_a-z0-9'+*$^&%=~!?{}]+(?:\.[-_a-z0-9'+*$^&%=~!?{}]+)*@(?:(?![-.])[-a-z0-9.]+(?<![-.])\.[a-z]{2,6}|\d{1,3}(?:\.\d{1,3}){3})(?::\d+)?)>",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
            );

    // Just an email address
    private static readonly Regex _explicit =
        new Regex(
            @"^[-_a-z0-9'+*$^&%=~!?{}]+(?:\.[-_a-z0-9'+*$^&%=~!?{}]+)*@(?:(?![-.])[-a-z0-9.]+(?<![-.])\.[a-z]{2,6}|\d{1,3}(?:\.\d{1,3}){3})(?::\d+)?$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
            );

    public override bool IsSatisfiedBy(string instance)
    {
        var result = _explicit.IsMatch(instance) || _names.IsMatch(instance);
        return result;
    }
}

[/code:c#]

### Writing a Custom Validation Attribute

You can supply your own transformation or validation behaviors by extending the abstract ValidationAttribute class and providing an 
implementation for the single method defined. The next code example shows a custom implementation of a validation attribute that 
uppercases a string value, and converts null values to string literals. The value passed to TransformValue could be _null_, so your 
validation attribute should account for this.

### Combining Method and Attribute Styles

You can use a combination of method and attribute style programming to build your requests, just keep in mind that Hammock will 
honor the RestClient and then RestRequest values you specify before using attributes. In other words, if you specify a parameter or 
user agent attribute on a class with IWebQueryInfo, that has the same name as a parameter you pass to RestClient, the value of that 
parameter or the user agent will take the RestClient value, not the attribute value. Headers will continue to combine values from all 
sources.



## Sending Web API Requests

After your request is ready for transport, you can execute it sequentially or asynchronously to return a RestResponse object. 
RestResponse is also available as a generic class scoped to the expected return value. This is useful if you are expecting a response 
entity that you can deserialize into a common class, making the process of converting content from the HTTP seam to regular .NET 
classes transparent; in this case the object is available in RestResponse<T>’s ContentEntity property. If you do not specify an 
IDeserializer on RestRequest or RestClient, but you expect a ContentEntity anyway, it will return a null value. If you are not 
intending to capture strongly typed responses, you can obtain the raw results from the Content property or cast the ContentEntity 
yourself provided you also supplied a ResponseEntityType along with your non-generic request, otherwise the deserializer would not know 
which target type you intended. Other diagnostic information about the request is available in the RestResponse class, and Hammock 
will output essential information about requests and responses through .NET’s tracing capability, so you can attach your own listeners 
to log important details.

[]

### Basic Operation

There are two methods available to send sequential requests when your application will wait for the results: Request, and Request<T>. 
The only difference is whether you expect to deserialize the response body as a .NET class before building the RestResponse object.

[code:c#]
using System;
using System.IO;
using Hammock.Serialization;
using Newtonsoft.Json;

namespace Hammock.Extras
{
    public class HammockJsonDotNetSerializer : ISerializer, IDeserializer
    {
        private readonly JsonSerializer _serializer;

        public HammockJsonDotNetSerializer(JsonSerializerSettings settings)
        {
            _serializer = new JsonSerializer
            {
                ConstructorHandling = settings.ConstructorHandling,
                ContractResolver = settings.ContractResolver,
                ObjectCreationHandling = settings.ObjectCreationHandling,
                MissingMemberHandling = settings.MissingMemberHandling,
                DefaultValueHandling = settings.DefaultValueHandling,
                NullValueHandling = settings.NullValueHandling
            };

            foreach (var converter in settings.Converters)
            {
                _serializer.Converters.Add(converter);
            }
        }

        #region IDeserializer Members

        public virtual object Deserialize(string content, Type type)
        {
            using (var sr = new StringReader(content))
            {
                using (var jtr = new JsonTextReader(sr))
                {
                    return _serializer.Deserialize(jtr, type);
                }
            }
        }

        public virtual T Deserialize<T>(string content)
        {
            using (var sr = new StringReader(content))
            {
                using (var jtr = new JsonTextReader(sr))
                {
                    return _serializer.Deserialize<T>(jtr);
                }
            }
        }

        #endregion

        #region ISerializer Members

        public virtual string Serialize(object instance, Type type)
        {
            using (var sw = new StringWriter())
            {
                using (var jtw = new JsonTextWriter(sw))
                {
                    jtw = Formatting.Indented;
                    jtw = '"';

                    _serializer.Serialize(jtw, instance);
                    var result = sw();
                    return result;
                }
            }
        }

        public virtual string ContentType
        {
            get { return "application/json"; }
        }

        public virtual Encoding ContentEncoding
        {
            get { return Encoding.UTF8; }
        }

        #endregion
    }
}
[/code:c#]

### Asynchronous Operation
[]

### Using RetryPolicy and Timeout
[]

### Caching Responses
[]

### Mocking Responses
[]

### Authenticating Requests
[]

#### Using BasicAuthCredentials
[]

#### Using OAuthCredentials
[]

#### Creating Custom Credentials
[]

### Request Tracing
[]

### Entity Serialization

Hammock provides hooks to allow you to serialize objects into your preferred format, usually JSON or XML when sending a POST or PUT 
request. This is a common scenario in many APIs that expect an entity when creating or updating records. Similarly, you can attach a 
deserializer to automatically convert a server entity into a regular class, to make it easy to work with in your code.

Hammock doesn’t push a particular serialization strategy on you. There are already too many ways to serialize and deserialize objects 
(communiy favorites are JSON.NET http://codeplex.com/json and ServiceStack TypeSerializer http://code.google.com/p/servicestack/wiki/TypeSerializer, 
and we don’t want to introduce another one.

You decide how you will serialize or deserialize your entities, and provide an implementation of ISerializer and IDeserializer to do
the work. That said, Hammock does include stock .NET serializers that will serialize and deserialize entities based on 
DataContractSerializer, DataContractJsonSerializer, JavaScriptSerializer, and XmlSerializer, so if you are already using those schemes 
in your code, hooking them up is straightforward.

#### Creating Custom Serializers

If you are using a different serialization scheme, you will need to provide an implementation of ISerializer, IDeserializer, or both, 
typically in the same concrete class, depending on your needs. One popular serialization library for JSON (with support for XML) is 
JSON.NET, which is used in TweetSharp. Here is an example implementation of a custom Hammock serializer that will let you use JSON.NET 
to serialize and deserialize your request and response entities, respectively. This example is provided in the Hammock.Extras project 
included with the source code.

[code:c#]
[/code:c#]

#### Achieving POCO for XML and JSON Entities

#### Dynamic serialization in .NET 4.0

### Periodic Tasks

### Rate Limiting

## Handling REST Responses
### Deserializing Responses
### Handling Errors

## Walkthrough: Postmark (JSON)

## Walkthrough: FreshBooks (XML)

## Walkthrough: Twitter (OAuth 1.0a)
### OAuth 1.0a Authentication
### Credential Proxies using OAuth Echo 

## Walkthrough: Facebook Graph (OAuth 2.0)
### OAuth 2.0 Authentication


