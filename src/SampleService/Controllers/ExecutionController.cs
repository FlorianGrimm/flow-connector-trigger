﻿using FlowTriggerManagingService;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SampleService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExecutionController : ControllerBase
    {
        private readonly ITriggerManagingService _triggerService;
        private static readonly HttpClient _client = new HttpClient();

        public ExecutionController(ITriggerManagingService triggerService)
        {
            _triggerService = triggerService;
        }

        // GET api/execution/connectors
        [HttpGet("connectors")]
        public IEnumerable<FlowTriggerDataContract> GetConnectors()
        {
            return _triggerService.ListAllTriggersAsync();
        }

        // POST api/execution/key
        [HttpPost("key")]
        public void UpdateProperties(string hookId, [FromBody]string key)
        {
            _triggerService.UpdateApiKeyAsync(hookId, key);
        }

        // POST api/execution/properties
        [HttpPost("properties")]
        public async Task UpdatePropertiesAsync(string hookId, [FromBody]IEnumerable<string> properties, string hookName = null)
        {
            await _triggerService.UpdatePropertiesAsync(hookId, hookName, properties);
        }

        // POST api/execution/dynamic?hookId={hookId}
        [HttpPost("dynamic")]
        public async Task<object> ExecDynamicAsync(string hookId, [FromBody]List<string> values)
        {
            var callback = await _triggerService.GetCallbackAsync(hookId);
            if (callback != null)
            {
                var properties = await _triggerService.GetPropertiesAsync(hookId);
                if (properties.Count() != values.Count())
                {
                    return "Properties' count is not matching values' count.";
                }
                dynamic obj = new JObject();
                int i = 0;
                foreach (var prop in properties)
                {
                    obj[prop] = values[i];
                    i++;
                }
                var req = JsonConvert.SerializeObject(obj);
                var res = await _client.PostAsync(callback, new StringContent(req, Encoding.UTF8, "application/json"));
                return null;
            }
            return "No call back.";
        }

        // POST api/execution/fixed?hookId={hookId}&para1={para1}&para2={para2}
        [HttpPost("fixed")]
        public async Task ExecAsync(string hookId, string para1, string para2)
        {
            var callback = await _triggerService.GetCallbackAsync(hookId);
            if (callback != null)
            {
                var req = new WebhookResponse() { Parameter1 = para1, Parameter2 = para2 };
                var res = await _client.PostAsync(callback, new StringContent(JsonConvert.SerializeObject(req), Encoding.UTF8, "application/json"));
            }
        }
    }
}
