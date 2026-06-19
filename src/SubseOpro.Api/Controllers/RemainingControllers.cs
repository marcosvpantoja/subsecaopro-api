using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubseOpro.Api.Services;
using SubseOpro.Application.Mappers;
using SubseOpro.Application.ViewModels.Post;
using SubseOpro.Application.ViewModels.Social;
using SubseOpro.Domain.Entities;
using SubseOpro.Domain.Interfaces;

namespace SubseOpro.Api.Controllers;

/// <summary>
/// Gerencia postagens públicas e administrativas.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IRepositoryFactory _factory;
    private readonly ITenantContext _tenantContext;

    public PostsController(IRepositoryFactory factory, ITenantContext tenantContext)
    {
        _factory = factory;
        _tenantContext = tenantContext;
    }

    /// <summary>
    /// Lista posts ativos para consumo público.
    /// </summary>
    /// <remarks>
    /// Validação manual: GET /api/v1/posts sem autenticação.
    /// Sucesso esperado: 200 com lista de posts ativos.
    /// </remarks>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Posts ativos.</returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<PostViewModel>>> GetAll(CancellationToken ct)
    {
        var entities = await _factory.Posts.GetRecentAsync(ct);
        return Ok(PostMapper.ToViewModelList(entities.Where(p => p.IsActive)));
    }

    /// <summary>
    /// Lista posts para gestão no backoffice.
    /// </summary>
    /// <remarks>
    /// Validação manual: GET /api/v1/posts/admin com token Backoffice.
    /// Sucesso esperado: 200 com todos os posts. Falha esperada: 401 sem token.
    /// </remarks>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Posts para administração.</returns>
    [HttpGet("admin")]
    [Authorize(Policy = "Backoffice")]
    public async Task<ActionResult<List<PostViewModel>>> GetAllForBackoffice(CancellationToken ct)
    {
        var entities = await _factory.Posts.GetRecentAsync(ct);
        return Ok(PostMapper.ToViewModelList(entities));
    }

    /// <summary>
    /// Cria um novo post.
    /// </summary>
    /// <remarks>
    /// Validação manual: POST /api/v1/posts com título e token Backoffice.
    /// Sucesso esperado: 201 com post criado. Falhas esperadas: 400 payload inválido, 401 sem token.
    /// </remarks>
    /// <param name="request">Dados do post.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Post criado.</returns>
    [HttpPost]
    [Authorize(Policy = "Backoffice")]
    public async Task<ActionResult<PostViewModel>> Create([FromBody] UpsertPostRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { detail = "Título é obrigatório." });

        var entity = new Post
        {
            Id = Guid.NewGuid(),
            SubsectionId = _tenantContext.RequireSubsectionId(),
            PublishedAt = DateTime.UtcNow,
            Title = request.Title.Trim(),
            Caption = string.IsNullOrWhiteSpace(request.Caption) ? request.Title.Trim() : request.Caption.Trim(),
            LinkUrl = string.IsNullOrWhiteSpace(request.Link) ? null : request.Link.Trim(),
            Source = PostMapper.ParseSource(request.Source),
            IsActive = request.IsActive,
            PhotoUrl = request.Photo ?? request.ImageUrl ?? string.Empty,
            ImageGradient = string.IsNullOrWhiteSpace(request.Image) ? "from-blue-400 to-indigo-500" : request.Image,
            LikesCount = 0,
        };

        await _factory.Posts.AddAsync(entity, ct);
        return CreatedAtAction(nameof(GetAllForBackoffice), new { id = entity.Id }, PostMapper.ToViewModel(entity));
    }

    /// <summary>
    /// Atualiza um post existente.
    /// </summary>
    /// <remarks>
    /// Validação manual: PUT /api/v1/posts/{id} com token Backoffice.
    /// Sucesso esperado: 200 com post atualizado. Falhas esperadas: 400 GUID inválido, 404 não encontrado, 401 sem token.
    /// </remarks>
    /// <param name="id">Identificador GUID do post.</param>
    /// <param name="request">Dados de atualização.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Post atualizado.</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "Backoffice")]
    public async Task<ActionResult<PostViewModel>> Update(string id, [FromBody] UpsertPostRequest request, CancellationToken ct)
    {
        if (!Guid.TryParse(id, out var postId)) return BadRequest();
        var entity = await _factory.Posts.GetByIdAsync(postId, ct);
        if (entity is null) return NotFound();

        entity.Title = string.IsNullOrWhiteSpace(request.Title) ? entity.Title : request.Title.Trim();
        entity.Caption = string.IsNullOrWhiteSpace(request.Caption) ? entity.Caption : request.Caption.Trim();
        entity.LinkUrl = string.IsNullOrWhiteSpace(request.Link) ? null : request.Link.Trim();
        entity.Source = PostMapper.ParseSource(request.Source);
        entity.IsActive = request.IsActive;
        entity.PhotoUrl = request.Photo ?? request.ImageUrl ?? entity.PhotoUrl;
        entity.ImageGradient = string.IsNullOrWhiteSpace(request.Image) ? entity.ImageGradient : request.Image;

        await _factory.Posts.UpdateAsync(entity, ct);
        var updated = await _factory.Posts.GetByIdNoTrackingAsync(postId, ct);
        if (updated is null) return NotFound();
        return Ok(PostMapper.ToViewModel(updated));
    }

    /// <summary>
    /// Remove um post pelo identificador.
    /// </summary>
    /// <remarks>
    /// Validação manual: DELETE /api/v1/posts/{id} com token Backoffice.
    /// Sucesso esperado: 204. Falhas esperadas: 400 GUID inválido, 404 não encontrado, 401 sem token.
    /// </remarks>
    /// <param name="id">Identificador GUID do post.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Resultado sem conteúdo quando removido.</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "Backoffice")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        if (!Guid.TryParse(id, out var postId)) return BadRequest();
        var entity = await _factory.Posts.GetByIdAsync(postId, ct);
        if (entity is null) return NotFound();
        await _factory.Posts.DeleteAsync(entity, ct);
        return NoContent();
    }

    /// <summary>
    /// Registra curtida em um post.
    /// </summary>
    /// <remarks>
    /// Validação manual: POST /api/v1/posts/{id}/like com token válido.
    /// Sucesso esperado: 200 com contagem atualizada. Falhas esperadas: 400 GUID inválido, 401 sem token.
    /// </remarks>
    /// <param name="id">Identificador GUID do post.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Post com total de curtidas atualizado.</returns>
    [HttpPost("{id}/like")]
    [Authorize]
    public async Task<ActionResult<PostViewModel>> Like(string id, CancellationToken ct)
    {
        if (!Guid.TryParse(id, out var postId)) return BadRequest();

        var entity = await _factory.Posts.LikePostAsync(postId, ct);
        return Ok(PostMapper.ToViewModel(entity));
    }
}

/// <summary>
/// Gerencia contas sociais integradas ao sistema.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Policy = "Backoffice")]
public class SocialAccountsController : ControllerBase
{
    private readonly IRepositoryFactory _factory;
    private readonly ITenantContext _tenantContext;

    public SocialAccountsController(IRepositoryFactory factory, ITenantContext tenantContext)
    {
        _factory = factory;
        _tenantContext = tenantContext;
    }

    /// <summary>
    /// Lista contas sociais cadastradas.
    /// </summary>
    /// <remarks>
    /// Validação manual: GET /api/v1/socialaccounts com token Backoffice.
    /// Sucesso esperado: 200 com lista ordenada. Falha esperada: 401 sem token.
    /// </remarks>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Lista de contas sociais.</returns>
    [HttpGet]
    public async Task<ActionResult<List<SocialAccountViewModel>>> GetAll(CancellationToken ct)
    {
        var entities = await _factory.SocialAccounts.GetOrderedAsync(ct);
        return Ok(SocialAccountMapper.ToViewModelList(entities));
    }

    /// <summary>
    /// Cria uma conta social.
    /// </summary>
    /// <remarks>
    /// Validação manual: POST /api/v1/socialaccounts com platform e handle.
    /// Sucesso esperado: 201 com conta criada. Falhas esperadas: 400 payload inválido, 401 sem token.
    /// </remarks>
    /// <param name="request">Dados da conta social.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Conta social criada.</returns>
    [HttpPost]
    public async Task<ActionResult<SocialAccountViewModel>> Create([FromBody] UpsertSocialAccountRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Platform) || string.IsNullOrWhiteSpace(request.Handle))
            return BadRequest(new { detail = "Platform e handle são obrigatórios." });

        var entity = new SocialAccount
        {
            Id = Guid.NewGuid(),
            SubsectionId = _tenantContext.RequireSubsectionId(),
            Platform = request.Platform.Trim().ToLowerInvariant(),
            Handle = request.Handle.Trim(),
            ProfileUrl = request.ProfileUrl?.Trim() ?? string.Empty,
            Connected = request.Connected,
            FollowersCount = request.FollowersCount,
            LastSync = request.LastSync,
        };

        await _factory.SocialAccounts.AddAsync(entity, ct);
        return CreatedAtAction(nameof(GetAll), new { id = entity.Id }, SocialAccountMapper.ToViewModel(entity));
    }

    /// <summary>
    /// Atualiza uma conta social existente.
    /// </summary>
    /// <remarks>
    /// Validação manual: PUT /api/v1/socialaccounts/{id} com token Backoffice.
    /// Sucesso esperado: 200 com conta atualizada. Falhas esperadas: 400 GUID inválido, 404 não encontrada, 401 sem token.
    /// </remarks>
    /// <param name="id">Identificador GUID da conta social.</param>
    /// <param name="request">Dados para atualização.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Conta social atualizada.</returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<SocialAccountViewModel>> Update(string id, [FromBody] UpsertSocialAccountRequest request, CancellationToken ct)
    {
        if (!Guid.TryParse(id, out var accountId)) return BadRequest();
        var entity = await _factory.SocialAccounts.GetByIdAsync(accountId, ct);
        if (entity is null) return NotFound();

        entity.Platform = string.IsNullOrWhiteSpace(request.Platform) ? entity.Platform : request.Platform.Trim().ToLowerInvariant();
        entity.Handle = string.IsNullOrWhiteSpace(request.Handle) ? entity.Handle : request.Handle.Trim();
        entity.ProfileUrl = request.ProfileUrl?.Trim() ?? entity.ProfileUrl;
        entity.Connected = request.Connected;
        entity.FollowersCount = request.FollowersCount;
        entity.LastSync = request.LastSync;

        await _factory.SocialAccounts.UpdateAsync(entity, ct);
        return Ok(SocialAccountMapper.ToViewModel(entity));
    }

    /// <summary>
    /// Marca uma conta social como sincronizada.
    /// </summary>
    /// <remarks>
    /// Validação manual: POST /api/v1/socialaccounts/{id}/sync com token Backoffice.
    /// Sucesso esperado: 200 com data de sincronização atualizada. Falhas esperadas: 400 GUID inválido, 404 não encontrada.
    /// </remarks>
    /// <param name="id">Identificador GUID da conta social.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Conta social sincronizada.</returns>
    [HttpPost("{id}/sync")]
    public async Task<ActionResult<SocialAccountViewModel>> Sync(string id, CancellationToken ct)
    {
        if (!Guid.TryParse(id, out var accountId)) return BadRequest();
        var entity = await _factory.SocialAccounts.GetByIdAsync(accountId, ct);
        if (entity is null) return NotFound();

        entity.Connected = true;
        entity.LastSync = DateTime.UtcNow;
        await _factory.SocialAccounts.UpdateAsync(entity, ct);
        return Ok(SocialAccountMapper.ToViewModel(entity));
    }
}

/// <summary>
/// Gerencia banners exibidos no aplicativo.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class BannersController : ControllerBase
{
    private readonly IRepositoryFactory _factory;
    private readonly ITenantContext _tenantContext;

    public BannersController(IRepositoryFactory factory, ITenantContext tenantContext)
    {
        _factory = factory;
        _tenantContext = tenantContext;
    }

    /// <summary>
    /// Lista banners disponíveis.
    /// </summary>
    /// <remarks>
    /// Validação manual: GET /api/v1/banners sem autenticação.
    /// Sucesso esperado: 200 com banners ordenados.
    /// </remarks>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Lista de banners.</returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<SubseOpro.Application.ViewModels.Banner.BannerViewModel>>> GetAll(CancellationToken ct)
    {
        var entities = await _factory.Banners.GetOrderedAsync(ct);
        return Ok(BannerMapper.ToViewModelList(entities));
    }

    /// <summary>
    /// Cria um novo banner.
    /// </summary>
    /// <remarks>
    /// Validação manual: POST /api/v1/banners com token Backoffice e título.
    /// Sucesso esperado: 200 com banner criado. Falhas esperadas: 400 título ausente, 401 sem token.
    /// </remarks>
    /// <param name="request">Dados do banner.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Banner criado.</returns>
    [HttpPost]
    [Authorize(Policy = "Backoffice")]
    public async Task<ActionResult<SubseOpro.Application.ViewModels.Banner.BannerViewModel>> Create(
        [FromBody] UpsertBannerRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { detail = "Título é obrigatório." });

        var entity = new Banner
        {
            Id = Guid.NewGuid(),
            SubsectionId = _tenantContext.RequireSubsectionId(),
            Title = request.Title.Trim(),
            Subtitle = request.Subtitle ?? string.Empty,
            Cta = request.Cta ?? string.Empty,
            Color = request.Color ?? "from-blue-700/85 to-indigo-900/85",
            PhotoUrl = request.Photo ?? string.Empty,
            LinkTo = string.IsNullOrWhiteSpace(request.To) ? "/club" : request.To,
            LinkParams = request.Params,
            SortOrder = request.SortOrder ?? 99,
        };

        await _factory.Banners.AddAsync(entity, ct);
        return Ok(BannerMapper.ToViewModel(entity));
    }

    /// <summary>
    /// Remove banner por identificador.
    /// </summary>
    /// <remarks>
    /// Validação manual: DELETE /api/v1/banners/{id} com token Backoffice.
    /// Sucesso esperado: 204. Falhas esperadas: 400 GUID inválido, 404 não encontrado, 401 sem token.
    /// </remarks>
    /// <param name="id">Identificador GUID do banner.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Resultado sem conteúdo quando removido.</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "Backoffice")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        if (!Guid.TryParse(id, out var bannerId)) return BadRequest();
        var entity = await _factory.Banners.GetByIdAsync(bannerId, ct);
        if (entity is null) return NotFound();
        await _factory.Banners.DeleteAsync(entity, ct);
        return NoContent();
    }
}

/// <summary>
/// Gerencia parceiros e convênios.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class PartnersController : ControllerBase
{
    private readonly IRepositoryFactory _factory;
    private readonly ITenantContext _tenantContext;

    public PartnersController(IRepositoryFactory factory, ITenantContext tenantContext)
    {
        _factory = factory;
        _tenantContext = tenantContext;
    }

    /// <summary>
    /// Lista parceiros, opcionalmente filtrando por categoria.
    /// </summary>
    /// <remarks>
    /// Validação manual: GET /api/v1/partners?category=servicos com token válido.
    /// Sucesso esperado: 200 com lista filtrada. Falha esperada: 401 sem token.
    /// </remarks>
    /// <param name="category">Categoria textual do parceiro.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Lista de parceiros.</returns>
    [HttpGet]
    public async Task<ActionResult<List<SubseOpro.Application.ViewModels.Partner.PartnerViewModel>>> GetAll(
        [FromQuery] string? category, CancellationToken ct)
    {
        var cat = PartnerMapper.ToCategoryEnum(category);
        var entities = await _factory.Partners.GetByCategoryAsync(cat, ct);
        return Ok(PartnerMapper.ToViewModelList(entities));
    }

    /// <summary>
    /// Obtém parceiro por ID.
    /// </summary>
    /// <remarks>
    /// Validação manual: GET /api/v1/partners/{id} com token válido.
    /// Sucesso esperado: 200 com parceiro. Falhas esperadas: 400 GUID inválido, 404 não encontrado.
    /// </remarks>
    /// <param name="id">Identificador GUID do parceiro.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Parceiro encontrado.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<SubseOpro.Application.ViewModels.Partner.PartnerViewModel>> GetById(string id, CancellationToken ct)
    {
        if (!Guid.TryParse(id, out var guid)) return BadRequest();

        var entity = await _factory.Partners.GetByIdAsync(guid, ct);
        if (entity is null) return NotFound();

        return Ok(PartnerMapper.ToViewModel(entity));
    }

    /// <summary>
    /// Cria um parceiro.
    /// </summary>
    /// <remarks>
    /// Validação manual: POST /api/v1/partners com token Backoffice e dados do parceiro.
    /// Sucesso esperado: 200 com parceiro criado. Falhas esperadas: 401 sem token.
    /// </remarks>
    /// <param name="request">Dados do parceiro.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Parceiro criado.</returns>
    [HttpPost]
    [Authorize(Policy = "Backoffice")]
    public async Task<ActionResult<SubseOpro.Application.ViewModels.Partner.PartnerViewModel>> Create(
        [FromBody] UpsertPartnerRequest request, CancellationToken ct)
    {
        var category = PartnerMapper.ToCategoryEnum(request.Category) ?? SubseOpro.Domain.Enums.PartnerCategory.Servicos;
        var couponCode = string.IsNullOrWhiteSpace(request.CouponCode)
            ? PartnerMapper.GenerateCouponCode(request.Name)
            : request.CouponCode.Trim().ToUpperInvariant();
        var entity = new Partner
        {
            Id = Guid.NewGuid(),
            SubsectionId = _tenantContext.RequireSubsectionId(),
            Name = request.Name.Trim(),
            Category = category,
            Discount = request.Discount ?? "0%",
            Description = request.Description ?? string.Empty,
            ValidUntil = request.ValidUntil ?? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(6)),
            Color = request.Color ?? "from-slate-500 to-slate-700",
            CouponCode = couponCode,
            CreatedAt = DateTime.UtcNow,
        };

        await _factory.Partners.AddAsync(entity, ct);
        return Ok(PartnerMapper.ToViewModel(entity));
    }

    /// <summary>
    /// Atualiza dados de um parceiro.
    /// </summary>
    /// <remarks>
    /// Validação manual: PUT /api/v1/partners/{id} com token Backoffice.
    /// Sucesso esperado: 200 com parceiro atualizado. Falhas esperadas: 400 GUID inválido, 404 não encontrado, 401 sem token.
    /// </remarks>
    /// <param name="id">Identificador GUID do parceiro.</param>
    /// <param name="request">Dados para atualização.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Parceiro atualizado.</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "Backoffice")]
    public async Task<ActionResult<SubseOpro.Application.ViewModels.Partner.PartnerViewModel>> Update(
        string id, [FromBody] UpsertPartnerRequest request, CancellationToken ct)
    {
        if (!Guid.TryParse(id, out var partnerId)) return BadRequest();
        var entity = await _factory.Partners.GetByIdAsync(partnerId, ct);
        if (entity is null) return NotFound();

        entity.Name = string.IsNullOrWhiteSpace(request.Name) ? entity.Name : request.Name.Trim();
        entity.Category = PartnerMapper.ToCategoryEnum(request.Category) ?? entity.Category;
        entity.Discount = string.IsNullOrWhiteSpace(request.Discount) ? entity.Discount : request.Discount;
        entity.Description = request.Description ?? entity.Description;
        entity.ValidUntil = request.ValidUntil ?? entity.ValidUntil;
        entity.Color = request.Color ?? entity.Color;
        if (!string.IsNullOrWhiteSpace(request.CouponCode))
            entity.CouponCode = request.CouponCode.Trim().ToUpperInvariant();

        await _factory.Partners.UpdateAsync(entity, ct);
        return Ok(PartnerMapper.ToViewModel(entity));
    }

    /// <summary>
    /// Exclui parceiro por identificador.
    /// </summary>
    /// <remarks>
    /// Validação manual: DELETE /api/v1/partners/{id} com token Backoffice.
    /// Sucesso esperado: 204. Falhas esperadas: 400 GUID inválido, 404 não encontrado, 401 sem token.
    /// </remarks>
    /// <param name="id">Identificador GUID do parceiro.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Resultado sem conteúdo quando removido.</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "Backoffice")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        if (!Guid.TryParse(id, out var partnerId)) return BadRequest();
        var entity = await _factory.Partners.GetByIdAsync(partnerId, ct);
        if (entity is null) return NotFound();
        await _factory.Partners.DeleteAsync(entity, ct);
        return NoContent();
    }
}

/// <summary>
/// Endpoints de notificações e marcação de leitura.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly IRepositoryFactory _factory;
    private readonly ITenantContext _tenantContext;

    public NotificationsController(IRepositoryFactory factory, ITenantContext tenantContext)
    {
        _factory = factory;
        _tenantContext = tenantContext;
    }

    /// <summary>
    /// Lista notificações visíveis para o contexto informado.
    /// </summary>
    /// <remarks>
    /// Validação manual: GET /api/v1/notifications?isLawyer=true.
    /// Sucesso esperado: 200 com notificações filtradas.
    /// </remarks>
    /// <param name="isLawyer">Quando verdadeiro, aplica filtro para advogados.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Lista de notificações visíveis.</returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<SubseOpro.Application.ViewModels.Notification.NotificationViewModel>>> GetAll(
        [FromQuery] bool isLawyer = false, CancellationToken ct = default)
    {
        var entities = await _factory.Notifications.GetVisibleAsync(isLawyer, ct);
        return Ok(NotificationMapper.ToViewModelList(entities));
    }

    /// <summary>
    /// Lista notificações não lidas do usuário autenticado.
    /// </summary>
    /// <remarks>
    /// Validação manual: GET /api/v1/notifications/inbox com token válido.
    /// Sucesso esperado: 200 com notificações não lidas. Falha esperada: 401 sem token.
    /// </remarks>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Inbox não lida do usuário.</returns>
    [HttpGet("inbox")]
    [Authorize]
    public async Task<ActionResult<List<SubseOpro.Application.ViewModels.Notification.NotificationViewModel>>> GetInbox(
        CancellationToken ct = default)
    {
        var userId = GetSubjectId();
        if (userId is null) return Unauthorized();

        var isLawyer = User.FindFirstValue("account_type") == "lawyer";
        var entities = await _factory.Notifications.GetUnreadForUserAsync(userId.Value, isLawyer, ct);
        return Ok(NotificationMapper.ToViewModelList(entities));
    }

    /// <summary>
    /// Marca todas as notificações do usuário como lidas.
    /// </summary>
    /// <remarks>
    /// Validação manual: POST /api/v1/notifications/read-all com token válido.
    /// Sucesso esperado: 200 com quantidade atualizada. Falha esperada: 401 sem token.
    /// </remarks>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Total de registros afetados.</returns>
    [HttpPost("read-all")]
    [Authorize]
    public async Task<ActionResult<object>> MarkAllAsRead(CancellationToken ct = default)
    {
        var userId = GetSubjectId();
        if (userId is null) return Unauthorized();

        var isLawyer = User.FindFirstValue("account_type") == "lawyer";
        var updated = await _factory.Notifications.MarkAllAsReadAsync(userId.Value, isLawyer, ct);
        return Ok(new { updated });
    }

    /// <summary>
    /// Marca uma notificação específica como lida.
    /// </summary>
    /// <remarks>
    /// Validação manual: POST /api/v1/notifications/read/{id} com token válido.
    /// Sucesso esperado: 200 com indicador de atualização. Falhas esperadas: 400 GUID inválido, 401 sem token.
    /// </remarks>
    /// <param name="id">Identificador GUID da notificação.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Indicador se houve atualização.</returns>
    [HttpPost("read/{id}")]
    [Authorize]
    public async Task<ActionResult<object>> MarkAsRead(string id, CancellationToken ct = default)
    {
        if (!Guid.TryParse(id, out var notificationId)) return BadRequest();

        var userId = GetSubjectId();
        if (userId is null) return Unauthorized();

        var isLawyer = User.FindFirstValue("account_type") == "lawyer";
        var updated = await _factory.Notifications.MarkAsReadAsync(userId.Value, notificationId, isLawyer, ct);
        return Ok(new { updated });
    }

    /// <summary>
    /// Cria uma notificação administrativa.
    /// </summary>
    /// <remarks>
    /// Validação manual: POST /api/v1/notifications com título e mensagem, usando token Backoffice.
    /// Sucesso esperado: 200 com notificação criada. Falhas esperadas: 400 payload inválido, 401 sem token.
    /// </remarks>
    /// <param name="request">Dados da notificação.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Notificação criada.</returns>
    [HttpPost]
    [Authorize(Policy = "Backoffice")]
    public async Task<ActionResult<SubseOpro.Application.ViewModels.Notification.NotificationViewModel>> Create(
        [FromBody] CreateNotificationRequest request,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Message))
            return BadRequest(new { detail = "Título e mensagem são obrigatórios." });

        var entity = new Notification
        {
            Id = Guid.NewGuid(),
            SubsectionId = _tenantContext.RequireSubsectionId(),
            Icon = string.IsNullOrWhiteSpace(request.Icon) ? "Bell" : request.Icon.Trim(),
            Text = $"{request.Title.Trim()}: {request.Message.Trim()}",
            CreatedAt = DateTime.UtcNow,
            ForLawyerOnly = request.Segment?.Trim().ToLowerInvariant() != "todos",
            Href = string.IsNullOrWhiteSpace(request.Href) ? "/notificacoes" : request.Href.Trim(),
        };

        await _factory.Notifications.AddAsync(entity, ct);
        return Ok(NotificationMapper.ToViewModel(entity));
    }

    private Guid? GetSubjectId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}

/// <summary>
/// Endpoints públicos e administrativos de artigos.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class ArticlesController : ControllerBase
{
    private readonly IRepositoryFactory _factory;
    private readonly ITenantContext _tenantContext;

    public ArticlesController(IRepositoryFactory factory, ITenantContext tenantContext)
    {
        _factory = factory;
        _tenantContext = tenantContext;
    }

    /// <summary>
    /// Lista todos os artigos.
    /// </summary>
    /// <remarks>
    /// Validação manual: GET /api/v1/articles sem autenticação.
    /// Sucesso esperado: 200 com lista de artigos.
    /// </remarks>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Lista de artigos.</returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<SubseOpro.Application.ViewModels.Article.ArticleViewModel>>> GetAll(CancellationToken ct)
    {
        var entities = await _factory.Articles.GetAllAsync(ct);
        return Ok(ArticleMapper.ToViewModelList(entities));
    }

    /// <summary>
    /// Obtém um artigo pelo slug.
    /// </summary>
    /// <remarks>
    /// Validação manual: GET /api/v1/articles/{slug} sem autenticação.
    /// Sucesso esperado: 200 com artigo. Falha esperada: 404 quando slug não existe.
    /// </remarks>
    /// <param name="slug">Slug único do artigo.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Artigo encontrado.</returns>
    [HttpGet("{slug}")]
    [AllowAnonymous]
    public async Task<ActionResult<SubseOpro.Application.ViewModels.Article.ArticleViewModel>> GetBySlug(string slug, CancellationToken ct)
    {
        var entity = await _factory.Articles.GetBySlugAsync(slug, ct);
        if (entity is null) return NotFound();

        return Ok(ArticleMapper.ToViewModel(entity));
    }

    /// <summary>
    /// Cria um novo artigo.
    /// </summary>
    /// <remarks>
    /// Validação manual: POST /api/v1/articles com token Backoffice e campos title/slug.
    /// Sucesso esperado: 200 com artigo criado. Falhas esperadas: 400 payload inválido, 409 slug duplicado, 401 sem token.
    /// </remarks>
    /// <param name="request">Dados do artigo.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Artigo criado.</returns>
    [HttpPost]
    [Authorize(Policy = "Backoffice")]
    public async Task<ActionResult<SubseOpro.Application.ViewModels.Article.ArticleViewModel>> Create(
        [FromBody] UpsertArticleRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Slug))
            return BadRequest(new { detail = "Título e slug são obrigatórios." });

        var exists = await _factory.Articles.GetBySlugAsync(request.Slug.Trim(), ct);
        if (exists is not null)
            return Conflict(new { detail = "Já existe um artigo com este slug." });

        var entity = new Article
        {
            Id = Guid.NewGuid(),
            SubsectionId = _tenantContext.RequireSubsectionId(),
            Slug = request.Slug.Trim(),
            Title = request.Title.Trim(),
            Icon = string.IsNullOrWhiteSpace(request.Icon) ? "📄" : request.Icon.Trim(),
            Summary = request.Summary?.Trim() ?? string.Empty,
            Content = request.Content?.Trim(),
        };

        await _factory.Articles.AddAsync(entity, ct);
        return Ok(ArticleMapper.ToViewModel(entity));
    }

    /// <summary>
    /// Atualiza um artigo existente.
    /// </summary>
    /// <remarks>
    /// Validação manual: PUT /api/v1/articles/{id} com token Backoffice.
    /// Sucesso esperado: 200 com artigo atualizado. Falhas esperadas: 400 GUID inválido, 404 não encontrado, 409 slug duplicado.
    /// </remarks>
    /// <param name="id">Identificador GUID do artigo.</param>
    /// <param name="request">Dados para atualização.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Artigo atualizado.</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "Backoffice")]
    public async Task<ActionResult<SubseOpro.Application.ViewModels.Article.ArticleViewModel>> Update(
        string id,
        [FromBody] UpsertArticleRequest request,
        CancellationToken ct)
    {
        if (!Guid.TryParse(id, out var articleId)) return BadRequest();

        var entity = await _factory.Articles.GetByIdAsync(articleId, ct);
        if (entity is null) return NotFound();

        if (!string.IsNullOrWhiteSpace(request.Slug) && !string.Equals(request.Slug.Trim(), entity.Slug, StringComparison.Ordinal))
        {
            var slugExists = await _factory.Articles.GetBySlugAsync(request.Slug.Trim(), ct);
            if (slugExists is not null && slugExists.Id != entity.Id)
                return Conflict(new { detail = "Já existe um artigo com este slug." });

            entity.Slug = request.Slug.Trim();
        }

        entity.Title = string.IsNullOrWhiteSpace(request.Title) ? entity.Title : request.Title.Trim();
        entity.Icon = string.IsNullOrWhiteSpace(request.Icon) ? entity.Icon : request.Icon.Trim();
        entity.Summary = request.Summary?.Trim() ?? entity.Summary;
        entity.Content = request.Content?.Trim() ?? entity.Content;

        await _factory.Articles.UpdateAsync(entity, ct);
        return Ok(ArticleMapper.ToViewModel(entity));
    }

    /// <summary>
    /// Exclui um artigo por identificador.
    /// </summary>
    /// <remarks>
    /// Validação manual: DELETE /api/v1/articles/{id} com token Backoffice.
    /// Sucesso esperado: 204. Falhas esperadas: 400 GUID inválido, 404 não encontrado, 401 sem token.
    /// </remarks>
    /// <param name="id">Identificador GUID do artigo.</param>
    /// <param name="ct">Token de cancelamento da requisição.</param>
    /// <returns>Resultado sem conteúdo quando removido.</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "Backoffice")]
    public async Task<IActionResult> Delete(string id, CancellationToken ct)
    {
        if (!Guid.TryParse(id, out var articleId)) return BadRequest();
        var entity = await _factory.Articles.GetByIdAsync(articleId, ct);
        if (entity is null) return NotFound();
        await _factory.Articles.DeleteAsync(entity, ct);
        return NoContent();
    }
}

/// <summary>
/// Endpoint de saúde da API.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Retorna status de saúde da aplicação.
    /// </summary>
    /// <remarks>
    /// Validação manual: GET /api/v1/health sem autenticação.
    /// Sucesso esperado: 200 com status healthy.
    /// </remarks>
    /// <returns>Status de saúde do serviço.</returns>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Get() => Ok(new { status = "healthy", service = "SubseOpro.Api" });
}

public class UpsertPostRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public string? Link { get; set; }
    public string Source { get; set; } = "manual";
    public bool IsActive { get; set; } = true;
    public string? ImageUrl { get; set; }
    public string? Photo { get; set; }
    public string? Image { get; set; }
}

public class UpsertSocialAccountRequest
{
    public string Platform { get; set; } = string.Empty;
    public string Handle { get; set; } = string.Empty;
    public string? ProfileUrl { get; set; }
    public bool Connected { get; set; }
    public int? FollowersCount { get; set; }
    public DateTime? LastSync { get; set; }
}

public class UpsertPartnerRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Discount { get; set; }
    public string? Description { get; set; }
    public DateOnly? ValidUntil { get; set; }
    public string? Color { get; set; }
    public string? CouponCode { get; set; }
}

public class UpsertBannerRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Cta { get; set; }
    public string? Color { get; set; }
    public string? Photo { get; set; }
    public string? To { get; set; }
    public Dictionary<string, string>? Params { get; set; }
    public int? SortOrder { get; set; }
}

public class CreateNotificationRequest
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Segment { get; set; }
    public string? Icon { get; set; }
    public string? Href { get; set; }
}

public class UpsertArticleRequest
{
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Content { get; set; }
}
