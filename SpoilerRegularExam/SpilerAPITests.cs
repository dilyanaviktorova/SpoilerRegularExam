using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;

namespace SpoilerRegularExam


{
    [TestFixture]
    public class SpoilerAPITests
    {
        private RestClient client;
        private static string createdSpoilerId;
        private static string baseURL = "https://d3s5nxhwblsjbi.cloudfront.net";

        [OneTimeSetUp]
        public void Setup()
        {
            
            string token = GetJwtToken("heartbeatsf", "82469314");

            
            var options = new RestClientOptions(baseURL)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);
        }

        
        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseURL);

            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            return json.GetProperty("accessToken").GetString();

             
        }


        [Test, Order(1)]
        public void CreateSpoilerWithRequiredFields_ShouldReturnCreated()
        {
            var newSpoiler = new
            {
                Title = "Didi",
                Description = "Miche Oka",
                Url = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(newSpoiler);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            createdSpoilerId = json.GetProperty("storyId").GetString();
            Assert.That(createdSpoilerId, Is.Not.Null.And.Not.Empty,
           "storyId was null or empty.");


            var msg = json.GetProperty("msg").GetString();
            Assert.That(msg, Is.EqualTo("Successfully created!"));

        }

        [Test, Order(2)]
        public void EditLastSpoiler_ShouldReturnOK()
        {
            var editedSpoiler = new
            {
                Title = "Didkata",
                Description = "Miche Okaa",
                Url = ""
            };

            var request = new RestRequest($"/api/Story/Edit/{createdSpoilerId}", Method.Put);
            request.AddJsonBody(editedSpoiler);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            var msg = json.GetProperty("msg").GetString();
            Assert.That(msg, Is.EqualTo("Successfully edited"));

        }

        [Test, Order(3)]
        public void GetAllSpoilers_ShouldReturnList()
        {
            var request = new RestRequest("/api/Story/All", Method.Get);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var spoilers = JsonSerializer.Deserialize<List<object>>(response.Content);
            Assert.That(spoilers, Is.Not.Empty);
        }

        [Test, Order(4)]
        public void DeleteSpoiler_ShouldReturnOk()
        {
            var request = new RestRequest($"/api/Story/Delete/{createdSpoilerId}", Method.Delete);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test, Order(5)]
        public void CreateStory_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            var unacceptableStory = new
            {
                Title = "",
                Description = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(unacceptableStory);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }


        [Test, Order(6)]
        public void EditNonExistingStory_ShouldReturnNotFound()
        {
            string fakeId = "123";

            var unacceptableStory = new
            {
                Title = "Pipi",
                Description = "Jojoba",
                Url = ""
            };
            var request = new RestRequest($"/api/Story/Edit/{fakeId}", Method.Put);
            request.AddJsonBody(unacceptableStory);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            Assert.That(response.Content, Does.Contain("No spoilers..."));
        }

        [Test, Order(7)]
        public void DeleteNonExistingStory_ShouldReturnBadRequest()
        {
            string fakeId = "123";
            var request = new RestRequest($"/api/Story/Delete/{fakeId}", Method.Delete);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            Assert.That(response.Content, Does.Contain("Unable to delete this story spoiler!"));
        }


        [OneTimeTearDown]
        public void Cleanup()
        {
            client?.Dispose();
        }

    }
}