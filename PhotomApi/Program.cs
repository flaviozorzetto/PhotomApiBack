using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PhotomApi.Config;
using PhotomApi.Context;
using PhotomApi.Interfaces;
using PhotomApi.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var awsCredentialOptions = new AwsCredentialOptions()
{
    AccessKey = builder.Configuration.GetValue<string>("AwsCredentials_AccessKey"),
    SecretKey = builder.Configuration.GetValue<string>("AwsCredentials_SecretKey")
};

var jwtCredentialOptions = new JwtCredentialOptions()
{
    Audience = builder.Configuration.GetValue<string>("Jwt_Audience"),
    ClientID = builder.Configuration.GetValue<string>("Jwt_ClientID"),
    ClientSecret = builder.Configuration.GetValue<string>("Jwt_ClientSecret"),
    Issuer = builder.Configuration.GetValue<string>("Jwt_Issuer"),
    Key = builder.Configuration.GetValue<string>("Jwt_Key"),
};

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(setup =>
{
    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        BearerFormat = "JWT",
        Name = "JWT Authentication",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Description = "Put **_ONLY_** your JWT Bearer token on textbox below!",

        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    setup.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

    setup.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });
});

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtCredentialOptions.Key)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidIssuer = jwtCredentialOptions.Issuer,
            ValidAudience = jwtCredentialOptions.Audience,
        };
    });

builder.Services.AddSingleton(awsCredentialOptions);
builder.Services.AddSingleton(jwtCredentialOptions);
builder.Services.AddSingleton<AmazonContext>();
builder.Services.AddScoped<IBucketService, BucketService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

app.Logger.LogInformation("AwsCredentialsEnv:" + awsCredentialOptions.ToString());
app.Logger.LogInformation("JwtCredentialsEnv:" + jwtCredentialOptions.ToString());

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "JWTAuthDemo v1"));
}

app.UseHttpsRedirection();
app.UseCors(x => x
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
