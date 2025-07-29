using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// ✅ Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.SetIsOriginAllowed(origin =>
            {
                // Allow any localhost port for development
                return origin.StartsWith("http://localhost:") ||
                       origin.StartsWith("https://localhost:") ||
                       origin.StartsWith("http://127.0.0.1:") ||
                       origin.StartsWith("https://127.0.0.1:");
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();

            //policy
            //.WithOrigins("http://localhost:62437")
            //.AllowAnyHeader()
            //.AllowAnyMethod()
            //.AllowCredentials();

            //policy
            //.WithOrigins("http://localhost:4200", "http://localhost:62437") // أضف كل الـ origins التي تستخدمها
            //.AllowAnyHeader()
            //.AllowAnyMethod()
            //.AllowCredentials(); // لو تستخدم كوكي أو توكن في الهيدر
        });
});

// ✅ Add Authentication
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// ✅ Add Authorization (يجب أن يكون قبل Build)
builder.Services.AddAuthorization();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 🔨 Build app
var app = builder.Build();

// Swagger middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Cross-Origin-Opener-Policy", "same-origin");
    context.Response.Headers.Add("Cross-Origin-Embedder-Policy", "require-corp");
    await next();
});


//builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

// Avoid redirect for preflight
app.Use(async (context, next) =>
{
    if (context.Request.Method == "OPTIONS")
    {
        context.Response.StatusCode = 200;
        await context.Response.CompleteAsync();
    }
    else
    {
        await next();
    }
});


// ✅ ترتيب الـ middleware مهم جدًا:
app.UseHttpsRedirection();

//builder.Services.AddHttpClient();


app.UseCors("AllowLocalhostFrontend");    // 👈 UseCors قبل UseAuthentication

app.UseAuthentication();           // 👈 UseAuthentication
app.UseAuthorization();            // 👈 UseAuthorization

app.MapControllers();

app.Run();
