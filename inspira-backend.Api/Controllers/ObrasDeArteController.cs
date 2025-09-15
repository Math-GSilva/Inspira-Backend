using inspira_backend.Application.DTOs;
using inspira_backend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace inspira_backend.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ObrasDeArteController : ControllerBase
    {
        private readonly IObraDeArteService _service;
        public ObrasDeArteController(IObraDeArteService service) { _service = service; }

        private Guid GetCurrentUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        [HttpPost]
        [Authorize(Roles = "Artista")]
        public async Task<IActionResult> Create([FromForm] CreateObraDeArteDto dto)
        {
            var obraCriada = await _service.CreateAsync(dto, GetCurrentUserId());
            if (obraCriada == null) return BadRequest("Dados inválidos para a criação da obra.");
            return CreatedAtAction(nameof(GetById), new { id = obraCriada.Id }, obraCriada);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var obras = await _service.GetAllAsync();
            return Ok(obras);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
            var obra = await _service.GetByIdAsync(id);
            if (obra == null) return NotFound();
            return Ok(obra);
        }

        [HttpGet("{id}/midia")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMidia(Guid id)
        {
            var (data, contentType) = await _service.GetMidiaByIdAsync(id);
            if (data == null || contentType == null)
            {
                return NotFound();
            }
            return File(data, contentType);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Artista")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateObraDeArteDto dto)
        {
            try
            {
                var obraAtualizada = await _service.UpdateAsync(id, dto, GetCurrentUserId());
                if (obraAtualizada == null) return NotFound();
                return Ok(obraAtualizada);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Artista")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var result = await _service.DeleteAsync(id, GetCurrentUserId());
                if (!result) return NotFound();
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }
    }
}
