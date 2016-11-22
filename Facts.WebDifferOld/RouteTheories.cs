using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Web;
using System.Web.Routing;
using Moq;
using WebDiffer;
using Xunit;
using Xunit.Extensions;

namespace Facts.WebDiffer
{
    public class RouteTheories
    {
        [Theory]
        [RouteTheoryData]
        public void Application_URLs_will_route_as_expected(
            HttpContextBase httpContext,
            object expectations)
        {
            var routes = new RouteCollection();
            DiffPlexWebsite.RegisterRoutes(routes);
            RouteData routeData = routes.GetRouteData(httpContext);

            Assert.NotNull(routeData);
            foreach (var expectation in GetPropertyValues(expectations))
            {
                Assert.Equal(expectation.Value, routeData.Values[expectation.Key]);
            }
        }

        private static IDictionary<string, object> GetPropertyValues(object @object)
        {
            var propertyValues = new Dictionary<string, object>();

            if (@object != null)
            {
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(@object);

                foreach (PropertyDescriptor property in properties)
                {
                    object val = property.GetValue(@object);
                    propertyValues.Add(property.Name, val);
                }
            }

            return propertyValues;
        }


        private class RouteTheoryDataAttribute : DataAttribute
        {
            public override IEnumerable<object[]> GetData(MethodInfo methodUnderTest, Type[] parameterTypes)
            {
                var routeData = new List<object[]>
                                    {
                                        new object[]
                                            {
                                                CreateHttpContext("~/"),
                                                new {controller = "diff", action = "index"}
                                            },
                                        new object[]
                                            {
                                                CreateHttpContext("~/diff"),
                                                new {controller = "diff", action = "diff"}
                                            }                                    
                                    };

                return routeData;
            }

            private static HttpContextBase CreateHttpContext(string appRelativeCurrentExecutionFilePath)
            {
                var moqHttpContext = new Mock<HttpContextBase>();
                var moqHttpRequest = new Mock<HttpRequestBase>();

                moqHttpContext.Setup(c => c.Request)
                    .Returns(moqHttpRequest.Object);
                moqHttpRequest.Setup(r => r.AppRelativeCurrentExecutionFilePath)
                    .Returns(appRelativeCurrentExecutionFilePath);

                return moqHttpContext.Object;
            }
        }
    }
}