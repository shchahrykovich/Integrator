using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace AzureEmu.Controllers
{
    public class BlobController : ControllerBase
    {
        private readonly IBlobServiceEngine _engine;

        public BlobController(IBlobServiceEngine engine)
        {
            _engine = engine;
        }

        [HttpGet]
        public IActionResult Get(String container, String blob)
        {
            if (_engine.ContainsBlob(container, blob))
            {
                var bytes = _engine.GetBlob(container, blob);
                HttpContext.Response.StatusCode = 200;
                HttpContext.Response.Headers.ContentLength = bytes.Length;
                HttpContext.Response.Headers.Add("Content-MD5", GetMd5Hash(bytes));
                HttpContext.Response.Headers.Add("Accept-Ranges", "bytes");
                HttpContext.Response.Headers.Add("Last-Modified", DateTime.UtcNow.ToString("R"));

                HttpContext.Response.Headers.Add("ETag", "0x8D3C825319A7E5B");
                HttpContext.Response.Headers.Add("x-ms-request-id:", Guid.NewGuid().ToString("D"));
                HttpContext.Response.Headers.Add("x-ms-version:", "2016-05-31");
                HttpContext.Response.Headers.Add("x-ms-lease-status", "unlocked");
                HttpContext.Response.Headers.Add("x-ms-lease-state", "available");
                HttpContext.Response.Headers.Add("x-ms-blob-type", "BlockBlob");
                HttpContext.Response.Headers.Add("x-ms-server-encrypted", "false");
                HttpContext.Response.Headers.Add("Date", DateTime.UtcNow.ToString("R"));

                return File(bytes, "application/octet-stream");
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        [HttpHead]
        public IActionResult Head(String container, String blob)
        {
            if (_engine.ContainsBlob(container, blob))
            {
                var bytes = _engine.GetBlob(container, blob);
                HttpContext.Response.Headers.ContentLength = bytes.Length;
                HttpContext.Response.ContentType = "application/octet-stream";
                HttpContext.Response.Headers.Add("Content-MD5", GetMd5Hash(bytes));
                HttpContext.Response.Headers.Add("Accept-Ranges", "bytes");
                HttpContext.Response.Headers.Add("Last-Modified", "Fri, 19 Aug 2016 11:37:26 GMT");

                HttpContext.Response.Headers.Add("ETag", "0x8D3C825319A7E5B");
                HttpContext.Response.Headers.Add("x-ms-request-id:", "57649b7b-001e-003f-45b1-20a574000000");
                HttpContext.Response.Headers.Add("x-ms-version:", "2016-05-31");
                HttpContext.Response.Headers.Add("x-ms-lease-status", "unlocked");
                HttpContext.Response.Headers.Add("x-ms-lease-state", "available");
                HttpContext.Response.Headers.Add("x-ms-blob-type", "BlockBlob");
                HttpContext.Response.Headers.Add("x-ms-server-encrypted", "false");
                HttpContext.Response.Headers.Add("Date", "Tue, 29 Aug 2017 10:30:04 GMT");

                return new StatusCodeResult(200);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        [HttpPut]
        public IActionResult Put(String container, String blob)
        {
            byte[] content;
            using (MemoryStream stream = new MemoryStream())
            {
                Request.Body.CopyTo(stream);
                content = stream.ToArray();
            }

            _engine.PutBlob(container, blob, content);

            HttpContext.Response.Headers.Add("Content-MD5", GetMd5Hash(content));
            HttpContext.Response.Headers.Add("Last-Modified", DateTime.UtcNow.ToString("R"));
            HttpContext.Response.Headers.Add("x-ms-request-id:", Guid.NewGuid().ToString("D"));
            HttpContext.Response.Headers.Add("Date", DateTime.UtcNow.ToString("R"));
            HttpContext.Response.Headers.Add("ETag", "0x8D3C825319A7E5B");
            HttpContext.Response.Headers.Add("x-ms-version:", "2017-04-17");
            HttpContext.Response.Headers.Add("x-ms-server-encrypted", "false");

            return new StatusCodeResult(201);
        }

        public static string GetMd5Hash(byte[] input)
        {
            using (var md5Check = MD5.Create())
            {
                md5Check.TransformBlock(input, 0, input.Length, null, 0);
                md5Check.TransformFinalBlock(new byte[0], 0, 0);

                byte[] hashBytes = md5Check.Hash;
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}