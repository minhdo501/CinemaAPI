using CinemaApi.Data;
using CinemaApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CinemaApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly CinemaDbContext _dbContext;

        public MoviesController(CinemaDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [Authorize]
        [HttpGet]
        public IActionResult Get(string sort, int? pageNumber, int? pageSize)
        {
            // Paging material
            var currentPageNumber = pageNumber ?? 1;
            var currentPageSize = pageSize ?? 5;

            var movies = from movie in _dbContext.Movies select new
            {
                Id = movie.Id,
                Name = movie.Name,
                Duration = movie.Duration,
                Language = movie.Language,
                Rating = movie.Rating,
                Genre = movie.Genre
            };
            
            switch (sort)
            {
                case "desc":
                    return Ok(movies.Skip((currentPageNumber - 1)* currentPageSize).Take(currentPageSize).OrderByDescending(m => m.Rating));
                case "asc":
                    return Ok(movies.Skip((currentPageNumber - 1) * currentPageSize).Take(currentPageSize).OrderBy(m => m.Rating));
                default:
                    // If pageNumber and pageSize are null, default result will be first page with 5 records
                    return Ok(movies.Skip((currentPageNumber - 1) * currentPageSize).Take(currentPageSize));
            }
        }

        [Authorize]
        [HttpGet("[action]/{id}")]
        public IActionResult GetById(int id)
        {
            var movie = _dbContext.Movies.Find(id);

            if (movie == null)
            {
                return NotFound();
            }
            return Ok(movie);
        }

        [Authorize]
        [HttpGet("[action]")]
        // Search movies by name
        public IActionResult SearchMovies(string movieName)
        {
            var movies = from movie in _dbContext.Movies
                         where movie.Name.StartsWith(movieName)
                         select new
                         {
                             Id = movie.Id,
                             Name = movie.Name,
                             Duration = movie.Duration,
                             Language = movie.Language,
                             Rating = movie.Rating,
                             Genre = movie.Genre
                         };
            return Ok(movies);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Post([FromForm] Movie movie)
        {
            var guid = Guid.NewGuid();
            var filePath = Path.Combine("wwwroot", guid + ".jpg");

            if (movie.Image != null)
            {
                var fileStream = new FileStream(filePath, FileMode.Create);
                movie.Image.CopyTo(fileStream);
            }
            movie.ImageUrl = filePath.Remove(0, 7);
            _dbContext.Movies.Add(movie);
            _dbContext.SaveChanges();

            return StatusCode(StatusCodes.Status201Created);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromForm] Movie movie)
        {
            var movieResult = _dbContext.Movies.Find(id);

            if (movieResult == null)
            {
                return NotFound("No record found for this Id");
            }
            else
            {
                var guid = new Guid();
                var filePath = Path.Combine("wwwroot", guid + ".jpg");

                if(movieResult.Image != null)
                {
                    var fileStream = new FileStream(filePath, FileMode.Create);
                    movie.Image.CopyTo(fileStream);
                    movie.ImageUrl = filePath.Remove(0, 7);
                }

                movieResult.Name = movie.Name;
                movieResult.Description = movie.Description;
                movieResult.Language = movie.Language;
                movieResult.Duration = movie.Duration;
                movieResult.PlayingDate = movie.PlayingDate;
                movieResult.PlayingTime = movie.PlayingTime;
                movieResult.Rating = movie.Rating;
                movieResult.Genre = movie.Genre;
                movieResult.TrailerUrl = movie.TrailerUrl;
                movieResult.TicketPrice = movie.TicketPrice;

                _dbContext.SaveChanges();
                return Ok("Record updated successfully");

            }
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var movie = _dbContext.Movies.Find(id);

            if (movie == null)
            {
                return NotFound("No record found for this Id");
            }
            else
            {
                _dbContext.Movies.Remove(movie);
                _dbContext.SaveChanges();
                return Ok("Record deleted");
            }
        }
    }
}
