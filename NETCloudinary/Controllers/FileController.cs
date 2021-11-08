using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace NETCloudinary.Controllers
{
    [Route("files")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly Cloudinary _cloudinary;
        public IConfiguration Configuration { get; }

        public FileController(Cloudinary cloudinary, IConfiguration configuration)
        {
            _cloudinary = cloudinary;
            Configuration = configuration;
        }

        [HttpPost("upload")]
        public ActionResult UploadFile([FromForm] List<IFormFile> files, [FromForm] string nameImage)
        {
            try
            {
                ImageUploadResult result = null;

                if (files == null)
                {
                    return BadRequest("indique el archivo");
                }

                foreach (var formFile in files)
                {
                    if (formFile.Length > 0)
                    {
                        var uploadParams = new ImageUploadParams()
                        {
                            File = new FileDescription(formFile.FileName, formFile.OpenReadStream()),
                            UseFilename = true,
                            UniqueFilename = true,
                            Overwrite = true,
                            PublicId = nameImage ?? formFile.FileName
                        };

                        result = _cloudinary.Upload(uploadParams);
                    }
                }

                if(result.Error != null && result.Error.Message != null)
                {
                    return BadRequest(result.Error.Message);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("image/{img}")]
        public ActionResult GetImage(string img)
        {
            SearchResult result = _cloudinary.Search().Expression("public_id=" + img).Execute();

            if (result.TotalCount == 0)
            {
                return NotFound();
            }

            return Ok(_cloudinary.Api.UrlImgUp.Secure(true).Transform(new Transformation().Width(100).Height(100).Crop("fit").Dpr(1.0)).BuildUrl(img + "." + result.Resources[0].Format));
        }



        [HttpGet("image/transformation")]
        public ActionResult Transformation([FromQuery(Name = "dpr")] double dpr = 1.0, [FromQuery(Name = "ancho")] int ancho = 280, [FromQuery(Name = "alto")] int alto = 380)
        {
            return base.Ok(
                new 
                {
                    baseSecureUrl = Configuration.GetValue<string>("AccountSettings:DeliverySecureBaseAddress"),
                    baseUrl = Configuration.GetValue<string>("AccountSettings:DeliveryBaseAddress"),
                    transformation = new Transformation().Width(ancho).Height(alto).Crop("fit").Dpr(dpr).Generate() 
                });
        }

        [HttpGet("test")]
        public ActionResult Test()
        {
            return base.Ok(
                new
                {
                    baseSecureUrl = Configuration.GetValue<string>("AccountSettings:DeliverySecureBaseAddress"),
                    baseUrl = Configuration.GetValue<string>("AccountSettings:DeliveryBaseAddress")
                });
        }
    }
}
