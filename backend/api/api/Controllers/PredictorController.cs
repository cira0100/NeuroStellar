﻿using api.Models;
using api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;

namespace api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PredictorController : Controller
    {
        private readonly IPredictorService _predictorService;
        private JwtToken jwtToken;

        public PredictorController(IPredictorService predictorService, IConfiguration configuration)
        {
            _predictorService = predictorService;
            jwtToken = new JwtToken(configuration);
        }

        // GET: api/<PredictorController>/mypredictors
        [HttpGet("mypredictors")]
        [Authorize(Roles = "User")]
        public ActionResult<List<Predictor>> Get()
        {
            string username;
            var header = Request.Headers[HeaderNames.Authorization];
            if (AuthenticationHeaderValue.TryParse(header, out var headerValue))
            {
                var scheme = headerValue.Scheme;
                var parameter = headerValue.Parameter;
                username = jwtToken.TokenToUsername(parameter);
                if (username == null)
                    return null;
            }
            else
                return BadRequest();

            return _predictorService.GetMyPredictors(username);
        }
        // GET: api/<PredictorController>/publicpredictors
        [HttpGet("publicpredictors")]
        public ActionResult<List<Predictor>> GetPublicPredictors()
        {
            return _predictorService.GetPublicPredictors();
        }

        // GET api/<PredictorController>/{name}
        [HttpGet("/{name}")]
        [Authorize(Roles = "User")]
        public ActionResult<Predictor> Get(string name)
        {
            string username;
            var header = Request.Headers[HeaderNames.Authorization];
            if (AuthenticationHeaderValue.TryParse(header, out var headerValue))
            {
                var scheme = headerValue.Scheme;
                var parameter = headerValue.Parameter;
                username = jwtToken.TokenToUsername(parameter);
                if (username == null)
                    return null;
            }
            else
                return BadRequest();

            var predictor = _predictorService.GetOnePredictor(username, name);

            if (predictor == null)
                return NotFound($"Predictor with name = {name} or user with username = {username} not found");

            return predictor;
        }

        // POST api/<PredictorController>/add
        [HttpPost("add")]
        [Authorize(Roles = "User")]
        public ActionResult<Predictor> Post([FromBody] Predictor predictor)
        {
            var existingModel = _predictorService.GetOnePredictor(predictor.username, predictor.name);

            if (existingModel != null)
                return NotFound($"Predictor with name = {predictor.name} exisits");
            else
            {
                _predictorService.Create(predictor);

                return CreatedAtAction(nameof(Get), new { id = predictor._id }, predictor);
            }
        }



        // PUT api/<PredictorController>/{name}
        [HttpPut("/{name}")]
        [Authorize(Roles = "User")]
        public ActionResult Put(string name, [FromBody] Predictor predictor)
        {
            string username;
            var header = Request.Headers[HeaderNames.Authorization];
            if (AuthenticationHeaderValue.TryParse(header, out var headerValue))
            {
                var scheme = headerValue.Scheme;
                var parameter = headerValue.Parameter;
                username = jwtToken.TokenToUsername(parameter);
                if (username == null)
                    return null;
            }
            else
                return BadRequest();

            var existingDataset = _predictorService.GetOnePredictor(username, name);

            //ne mora da se proverava
            if (existingDataset == null)
                return NotFound($"Predictor with name = {name} or user with username = {username} not found");

            _predictorService.Update(username, name, predictor);

            return Ok($"Predictor with name = {name} updated");
        }


        // DELETE api/<PredictorController>/name
        [HttpDelete("/{name}")]
        [Authorize(Roles = "User")]
        public ActionResult Delete(string name)
        {
            string username;
            var header = Request.Headers[HeaderNames.Authorization];
            if (AuthenticationHeaderValue.TryParse(header, out var headerValue))
            {
                var scheme = headerValue.Scheme;
                var parameter = headerValue.Parameter;
                username = jwtToken.TokenToUsername(parameter);
                if (username == null)
                    return null;
            }
            else
                return BadRequest();

            var predictor = _predictorService.GetOnePredictor(username, name);

            if (predictor == null)
                return NotFound($"Predictor with name = {name} or user with username = {username} not found");

            _predictorService.Delete(predictor.username, predictor.name);

            return Ok($"Predictor with name = {name} deleted");

        }




    }
}