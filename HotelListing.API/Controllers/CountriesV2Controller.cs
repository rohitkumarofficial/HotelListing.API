﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelListing.API.Data;
using HotelListing.API.Entities.Country;
using AutoMapper;
using HotelListing.API.Repository;
using HotelListing.API.Configurations.Contracts;
using Microsoft.AspNetCore.Authorization;
using HotelListing.API.Exceptions;
using HotelListing.API.Entities;
using Microsoft.AspNetCore.OData.Query;

namespace HotelListing.API.Controllers
{
    [Route("api/v{version:ApiVersion}/countries")]
    [ApiController]
    [ApiVersion("2.0")]
    public class CountriesV2Controller : ControllerBase
    {
       
        private readonly IMapper _mapper;
        private readonly ICountriesRepository _countriesRepository;

        public CountriesV2Controller(
            IMapper mapper,
            ICountriesRepository countriesRepository
            )
        {
          
            this._mapper = mapper;
            this._countriesRepository = countriesRepository;
        }

        [HttpGet]
        [EnableQuery]
        public async Task<ActionResult<PageResult<CountryDto>>> GetCountries()
        {

            var countries = await _countriesRepository.GetAllAsync();
            var records = _mapper.Map<List<CountryDto>>(countries);

            return Ok(records);
        }

        // GET: api/Countries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CountryDetailDto>> GetCountry(int id)
        {
            var country = await _countriesRepository.GetDetails(id);

            if(country == null ) {
                throw new NotFoundException(nameof(GetCountry), id);
            }

            var mappedCountry = _mapper.Map<CountryDetailDto>(country);

            if (country == null)
            {
                throw new NotFoundException(nameof(GetCountry), id);
            }

            return mappedCountry;
        }

        // PUT: api/Countries/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCountry(int id, UpdateCountryDto countryToUpdate)
        {
            if (id != countryToUpdate.Id)
            {
                return BadRequest("Invalid record Id");
            }

            var country = await _countriesRepository.GetAsync(id);

            if (country == null)
            {
                throw new NotFoundException(nameof(PutCountry), id);
            }

            _mapper.Map(countryToUpdate, country);

            try
            {
                await _countriesRepository.UpdateAsync(country);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await CountryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Countries
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Country>> PostCountry(CreateCountryDto createCountry)
        {
            var country = _mapper.Map<Country>(createCountry);

            var entity = await _countriesRepository.AddAsync(country);

            return entity;
        }

        // DELETE: api/Countries/5  
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteCountry(int id)
        {
           
            var country = await _countriesRepository.GetAsync(id);
            if (country == null)
            {
                throw new NotFoundException(nameof(DeleteCountry), id);
            }

            await _countriesRepository.DeleteAsync(id);

            return NoContent();
        }

        private async Task<bool> CountryExists(int id)
        {
            return await _countriesRepository.IsExists(id);
        }
    }
}
