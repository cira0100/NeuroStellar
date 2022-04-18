﻿using System;
using api.Interfaces;
using api.Models;
using MongoDB.Driver;

namespace api.Services
{
	public class ModelService : IModelService
    {
        private readonly IMongoCollection<Model> _model;

        public ModelService(IUserStoreDatabaseSettings settings, IMongoClient mongoClient)
        {
            var database = mongoClient.GetDatabase(settings.DatabaseName);
            _model = database.GetCollection<Model>(settings.ModelCollectionName);
        }

        public Model Create(Model model)
        {
            _model.InsertOne(model);
            return model;
        }
        public Model Replace(Model model)
        {
            _model.ReplaceOne(m => m._id == model._id, model);
            return model;
        }

        public void Delete(string userId, string name)
        {
            _model.DeleteOne(model => (model.uploaderId == userId && model.name == name));
        }

        public List<Model> GetMyModels(string userId)
        {
            return _model.Find(model => model.uploaderId == userId).ToList();
        }
        public List<Model> GetLatestModels(string userId)
        {
            List<Model> list = _model.Find(model => model.uploaderId == userId).ToList();

            list = list.OrderByDescending(model => model.lastUpdated).ToList();

            return list;
        }

        /*
        public List<Model> GetPublicModels()
        {
            return _model.Find(model => model.isPublic == true).ToList();
        }
        */
        public Model GetOneModel(string userId, string name)
        {
            return _model.Find(model => model.uploaderId == userId && model.name == name).FirstOrDefault();
        }

        public Model GetOneModel(string id)
        {
            return _model.Find(model => model._id == id).FirstOrDefault();
        }

        public void Update(string userId, string name, Model model)
        {
            _model.ReplaceOne(model => model.uploaderId == userId && model.name == name, model);
        }
        public void Update(string id, Model model)
        {
            _model.ReplaceOne(model => model._id == id, model);
        }
        //
        public bool CheckHyperparameters(int inputNeurons, int hiddenLayerNeurons, int hiddenLayers, int outputNeurons)
        {
            if (hiddenLayers <= 0 || hiddenLayerNeurons <= 0)
                return false;
            if (hiddenLayers > inputNeurons)
                return false;
            if (hiddenLayerNeurons <= 2 * inputNeurons || hiddenLayerNeurons <= (2 / 3) * inputNeurons + outputNeurons || (hiddenLayerNeurons <= Math.Max(inputNeurons, outputNeurons) && hiddenLayerNeurons >= Math.Min(inputNeurons, outputNeurons)))
                return true;
            return false;
        }

        public bool CheckDb()
        {
            Model? model = null;
            model = _model.Find(model => model.uploaderId == "Igrannonica").FirstOrDefault();

            if (model != null)
                return false;
            else
                return true;
        
        }
    }
}
