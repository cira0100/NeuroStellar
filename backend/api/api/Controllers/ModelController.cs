using api.Models;
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
    public class ModelController : ControllerBase
    {

        private IMlConnectionService _mlService;
        private readonly IDatasetService _datasetService;
        private readonly IFileService _fileService;
        private readonly IModelService _modelService;
        private readonly IExperimentService _experimentService;
        private IJwtToken jwtToken;
        private readonly IMlConnectionService _mlConnectionService;


        public ModelController(IMlConnectionService mlService, IModelService modelService, IMlConnectionService mlConnectionService, IDatasetService datasetService, IFileService fileService, IConfiguration configuration,IJwtToken token,IExperimentService experiment)
        {
            _mlService = mlService;
            _modelService = modelService;
            _datasetService = datasetService;
            _fileService = fileService;
            _experimentService = experiment;
            jwtToken = token;
            _mlConnectionService = mlConnectionService;
        }

        [HttpPost("sendModel")]
        [Authorize(Roles = "User,Guest")]
        public async Task<ActionResult<string>> Test([FromBody] Model model)
        {
            string uploaderId;
            var header = Request.Headers[HeaderNames.Authorization];
            if (AuthenticationHeaderValue.TryParse(header, out var headerValue))
            {
                var scheme = headerValue.Scheme;
                var parameter = headerValue.Parameter;
                uploaderId = jwtToken.TokenToId(parameter);
                if (uploaderId == null)
                    return BadRequest();
            }
            else
                return BadRequest();
            var experiment=_experimentService.Get(model.experimentId);
            var dataset = _datasetService.GetOneDataset(experiment.datasetId);
            var filepath = _fileService.GetFilePath(dataset.fileId, uploaderId);
            _mlService.TrainModel(model,experiment,filepath);//To do  Obavestiti korisnika kada se model istrenira
            return Ok();
        }

        // GET: api/<ModelController>/mymodels
        [HttpGet("mymodels")]
        [Authorize(Roles = "User")]
        public ActionResult<List<Model>> Get()
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

            return _modelService.GetMyModels(username);
        }

        // vraca svoj model prema nekom imenu
        // GET api/<ModelController>/{name}
        [HttpGet("{name}")]
        [Authorize(Roles = "User")]
        public ActionResult<Model> Get(string name)
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

            var model = _modelService.GetOneModel(username, name);

            if (model == null)
                return NotFound($"Model with name = {name} not found");

            return model;
        }

        //odraditi da vraca modele prema nekom imenu



        // moze da vraca sve modele pa da se ovde odradi orderByDesc
        //odraditi to i u Datasetove i Predictore
        // GET: api/<ModelController>/getlatestmodels/{number}
        [HttpGet("getlatestmodels/{latest}")]
        [Authorize(Roles = "User")]
        public ActionResult<List<Model>> GetLatestModels(int latest)
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

            //ako bude trebao ID, samo iz baze uzeti

            List<Model> lista = _modelService.GetLatestModels(username);

            List<Model> novaLista = new List<Model>();

            for (int i = 0; i < latest; i++)
                novaLista.Add(lista[i]);

            return novaLista;
        }




        // POST api/<ModelController>/add
        [HttpPost("add")]
        [Authorize(Roles = "User,Guest")]
        public ActionResult<Model> Post([FromBody] Model model)//, bool overwrite)
        {
            bool overwrite = false;
            //username="" ako je GUEST
            //Experiment e = _experimentService.Get(model.experimentId);
            //model.inputNeurons = e.inputColumns.Length;
            if (_modelService.CheckHyperparameters(1, model.hiddenLayerNeurons, model.hiddenLayers, model.outputNeurons) == false)
                return BadRequest("Bad parameters!");

            var existingModel = _modelService.GetOneModel(model.username, model.name);

            if (existingModel != null && !overwrite)
                return NotFound($"Model with name = {model.name} exisits");
            else
            {
                model.isTrained = false;
                _modelService.Create(model);
                //return Ok();
                if (existingModel == null)
                    _modelService.Create(model);
                else
                {
                    _modelService.Replace(model);
                }

                return CreatedAtAction(nameof(Get), new { id = model._id }, model);
            }
        }



        public void AddToEmptyDb()
        {

            if (_modelService.CheckDefaultModels())
            {
                Model model = new Model();

                model.username = "igrannonica";
                model.name = "Igrannonica model 1";
                model.description = "Opis modela 1";
                model.dateCreated = DateTime.Now;
                model.lastUpdated = DateTime.Now;
                model.experimentId = "";
                model.type = "";
                model.encoding = "";
                model.optimizer = "";
                model.lossFunction = "";
                model.hiddenLayerNeurons = 0;
                model.hiddenLayers = 0;
                model.batchSize = 0;
                model.outputNeurons = 0;
                //model.hiddenLayerActivationFunctions[] =
                model.outputLayerActivationFunction = "";
                //model.metrics =
                model.epochs = 0;
                model.isTrained = false;

                _modelService.Create(model);

                //dodaj javne datasetove
            }

        }



        // PUT api/<ModelController>/{name}
        [HttpPut("{name}")]
        [Authorize(Roles = "User")]
        public ActionResult Put(string name, [FromBody] Model model)
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


            var existingModel = _modelService.GetOneModel(username, name);

            if (existingModel == null)
                return NotFound($"Model with name = {name} or user with username = {username} not found");

            _modelService.Update(username, name, model);
            return NoContent();
        }

        // DELETE api/<ModelController>/name
        [HttpDelete("{name}")]
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

            var model = _modelService.GetOneModel(username, name);

            if (model == null)
                return NotFound($"Model with name = {name} or user with username = {username} not found");

            _modelService.Delete(model.username, model.name);

            return Ok($"Model with name = {name} deleted");

        }

    }
}
