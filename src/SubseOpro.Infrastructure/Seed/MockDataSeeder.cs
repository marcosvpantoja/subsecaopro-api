using Microsoft.EntityFrameworkCore;
using SubseOpro.Application.Mappers;
using SubseOpro.Application.Services;
using SubseOpro.Domain.Entities;
using SubseOpro.Domain.Enums;
using SubseOpro.Infrastructure.Persistence;

namespace SubseOpro.Infrastructure.Seed;

public class MockDataSeeder
{
    private static readonly string DemoPassword = Environment.GetEnvironmentVariable("SEED_DEMO_PASSWORD") ?? Guid.NewGuid().ToString("N");

    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public MockDataSeeder(AppDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await _context.Lawyers.AnyAsync(ct))
        {
            await EnsureDemoLoginAsync(ct);
            await EnsureBackofficeUsersAsync(ct);
            await EnsureSocialAccountsAsync(ct);
            await EnsureElectionAsync(ct);
            await EnsureSubsectionAsync(ct);
            return;
        }

        await SeedSubsectionAsync(ct);
        await SeedPlatformAdminAsync(ct);
        var specialties = await SeedSpecialtiesAsync(ct);
        await SeedLawyersAsync(specialties, ct);
        await SeedBackofficeUsersAsync(ct);
        await SeedRoomsAsync(ct);
        await SeedPostsAsync(ct);
        await SeedSocialAccountsAsync(ct);
        await SeedBannersAsync(ct);
        await SeedPartnersAsync(ct);
        await SeedNotificationsAsync(ct);
        await SeedArticlesAsync(ct);
        await SeedReservationsAsync(ct);
        await SeedElectionAsync(ct);

        await _context.SaveChangesAsync(ct);
    }

    private async Task SeedSubsectionAsync(CancellationToken ct)
    {
        if (await _context.Subsections.IgnoreQueryFilters().AnyAsync(s => s.Id == SeedIds.Subsection1, ct))
            return;

        await _context.Subsections.AddAsync(new Subsection
        {
            Id = SeedIds.Subsection1,
            Name = "OAB Cidade Exemplo",
            Slug = "cidade-exemplo",
            IsActive = true,
            PrimaryColor = "#1B3A6B",
            AccentColor = "#D4A843",
            Description = "A Subseção de Cidade Exemplo da OAB/SP representa os advogados da região, oferecendo serviços institucionais, salas para reuniões, clube de benefícios e canais digitais de atendimento.",
            Address = "Rua Exemplo, 100 — Cidade Exemplo/SP",
            Phone = "(19) 3889-0000",
            Email = "contato@oabcidadeexemplo.org.br",
            Hours = "Segunda a sexta, 9h às 18h",
            IaEnabled = true,
            IaDisclaimer = SubsectionIaDefaults.DefaultDisclaimer,
            IaMaxTurns = SubsectionIaDefaults.DefaultMaxTurns,
            IaAreasCobertasJson = SubsectionMapper.SerializeAreas(null),
            CreatedAt = DateTime.UtcNow,
        }, ct);
    }

    private async Task SeedPlatformAdminAsync(CancellationToken ct)
    {
        if (await _context.PlatformAdmins.AnyAsync(ct)) return;

        await _context.PlatformAdmins.AddAsync(new PlatformAdmin
        {
            Id = SeedIds.PlatformAdmin1,
            Name = "Platform Admin",
            Email = "platform@subsecao.pro",
            PasswordHash = _passwordHasher.Hash(DemoPassword),
            IsActive = true,
        }, ct);
    }

    private async Task EnsureSubsectionAsync(CancellationToken ct)
    {
        if (!await _context.Subsections.IgnoreQueryFilters().AnyAsync(ct))
        {
            await SeedSubsectionAsync(ct);
            await _context.SaveChangesAsync(ct);
        }

        if (!await _context.PlatformAdmins.AnyAsync(ct))
        {
            await SeedPlatformAdminAsync(ct);
            await _context.SaveChangesAsync(ct);
        }
    }

    private async Task EnsureDemoLoginAsync(CancellationToken ct)
    {
        var demoLawyer = await _context.Lawyers
            .FirstOrDefaultAsync(l => l.Id == SeedIds.Lawyer8, ct);

        if (demoLawyer is null) return;

        var changed = false;

        if (!demoLawyer.IsActive)
        {
            demoLawyer.IsActive = true;
            changed = true;
        }

        if (string.IsNullOrEmpty(demoLawyer.PasswordHash) ||
            _passwordHasher.Verify("demo123", demoLawyer.PasswordHash) ||
            !_passwordHasher.Verify(DemoPassword, demoLawyer.PasswordHash))
        {
            demoLawyer.PasswordHash = _passwordHasher.Hash(DemoPassword);
            changed = true;
        }

        if (changed)
            await _context.SaveChangesAsync(ct);
    }

    private async Task<Dictionary<string, Specialty>> SeedSpecialtiesAsync(CancellationToken ct)
    {
        var names = new[]
        {
            "Família", "Sucessões", "Trabalhista", "Civil", "Imobiliário", "Penal",
            "Tributário", "Empresarial", "Consumidor", "Previdenciário", "Digital", "Startups",
        };

        var map = new Dictionary<string, Specialty>();
        foreach (var name in names)
        {
            var spec = new Specialty { Id = Guid.NewGuid(), SubsectionId = SeedIds.Subsection1, Name = name };
            await _context.Specialties.AddAsync(spec, ct);
            map[name] = spec;
        }

        return map;
    }

    private async Task SeedLawyersAsync(Dictionary<string, Specialty> specialties, CancellationToken ct)
    {
        var demoHash = _passwordHasher.Hash(DemoPassword);

        var lawyers = new[]
        {
            (SeedIds.Lawyer1, "Dra. Ana Carolina Ferreira", "OAB/SP 234.567", "Centro", new[] { "Família", "Sucessões" },
                "Especialista em Direito de Família com 15 anos de experiência em divórcios consensuais, inventários e planejamento sucessório.",
                4.9m, true, "19999990001", "ana.ferreira@adv.exemplo.org.br", "Av. Rebouças, 1020 — Centro, Sumaré", "Seg–Sex, 9h–18h",
                "from-rose-400 to-rose-600", "AF", "https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?w=300&q=80&auto=format&fit=crop", ""),
            (SeedIds.Lawyer2, "Dr. Ricardo Alves Santos", "OAB/SP 189.432", "Maria Antônia", new[] { "Trabalhista" },
                "Defesa de trabalhadores em rescisões, verbas e ações coletivas. Mais de 500 causas trabalhistas conduzidas.",
                4.8m, true, "19999990002", "ricardo.santos@adv.exemplo.org.br", "Rua Humaitá, 233 — Maria Antônia, Sumaré", "Seg–Sáb, 8h–19h",
                "from-blue-400 to-blue-600", "RS", "https://images.unsplash.com/photo-1560250097-0b93528c311a?w=300&q=80&auto=format&fit=crop", ""),
            (SeedIds.Lawyer3, "Dra. Juliana Mendes Costa", "OAB/SP 312.890", "Nova Veneza", new[] { "Civil", "Imobiliário" },
                "Contratos, locações, regularização de imóveis e usucapião. Atendimento humanizado e técnico.",
                4.7m, false, "19999990003", "juliana.mendes@adv.exemplo.org.br", "Rua Orozimbo Maia, 88 — Nova Veneza, Sumaré", "Seg–Sex, 10h–17h",
                "from-emerald-400 to-emerald-600", "JM", "https://images.unsplash.com/photo-1580489944761-15a19d654956?w=300&q=80&auto=format&fit=crop", ""),
            (SeedIds.Lawyer4, "Dr. Marcos Henrique Lima", "OAB/SP 98.234", "Matão", new[] { "Penal" },
                "Defesa criminal estratégica em todas as instâncias. 25 anos de experiência em tribunais.",
                4.9m, true, "19999990004", "marcos.lima@adv.exemplo.org.br", "Rua Barão de Jaguara, 1500 — Matão, Sumaré", "Seg–Sex, 9h–18h",
                "from-slate-500 to-slate-700", "ML", "https://images.unsplash.com/photo-1556157382-97eda2d62296?w=300&q=80&auto=format&fit=crop", ""),
            (SeedIds.Lawyer5, "Dra. Fernanda Oliveira", "OAB/SP 267.123", "Picerno", new[] { "Tributário", "Empresarial" },
                "Consultoria tributária para empresas, recuperação de créditos e planejamento fiscal.",
                4.8m, true, "19999990005", "fernanda.oliveira@adv.exemplo.org.br", "Av. Invernada, 1200 — Picerno, Sumaré", "Seg–Sex, 9h–18h",
                "from-amber-400 to-amber-600", "FO", "https://images.unsplash.com/photo-1544005313-94ddf0286df2?w=300&q=80&auto=format&fit=crop", ""),
            (SeedIds.Lawyer6, "Dr. Gabriel Souza Pinto", "OAB/SP 145.678", "Parque Bandeirantes", new[] { "Consumidor" },
                "Defesa do consumidor em compras, viagens, planos de saúde e telefonia.",
                4.6m, true, "19999990006", "gabriel.pinto@adv.exemplo.org.br", "Rua Antônio Tedesco, 555 — Parque Bandeirantes, Sumaré", "Seg–Sex, 9h–17h",
                "from-cyan-400 to-cyan-600", "GP", "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=300&q=80&auto=format&fit=crop", ""),
            (SeedIds.Lawyer7, "Dra. Camila Rocha Neves", "OAB/SP 301.456", "Jardim Bordon", new[] { "Previdenciário" },
                "Aposentadorias, benefícios e revisões de INSS. Atendimento online em todo o Brasil.",
                4.9m, true, "19999990007", "camila.neves@adv.exemplo.org.br", "Av. Norte-Sul, 1900 — Jardim Bordon, Sumaré", "Seg–Sex, 8h–18h",
                "from-violet-400 to-violet-600", "CN", "https://images.unsplash.com/photo-1438761681033-6461ffad8d80?w=300&q=80&auto=format&fit=crop", ""),
            (SeedIds.Lawyer8, "Dr. Thiago Barbosa Reis", "OAB/SP 378.901", "Macieira", new[] { "Digital", "Startups" },
                "Direito digital, LGPD, contratos de tecnologia e estruturação de startups.",
                5.0m, true, "19999990008", "thiago.reis@adv.exemplo.org.br", "Rua Exemplo, 100 — Macieira, Sumaré", "Seg–Sex, 10h–19h",
                "from-indigo-400 to-indigo-600", "TR", "https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?w=300&q=80&auto=format&fit=crop", demoHash),
        };

        foreach (var (id, name, oab, city, specs, bio, rating, online, whatsapp, email, address, availability, color, initials, photo, hash) in lawyers)
        {
            var lawyer = new Lawyer
            {
                Id = id,
                SubsectionId = SeedIds.Subsection1,
                Name = name,
                OabNumber = oab,
                PasswordHash = hash,
                City = city,
                Bio = bio,
                Rating = rating,
                Online = online,
                Whatsapp = whatsapp,
                Email = email,
                Address = address,
                Availability = availability,
                Color = color,
                Initials = initials,
                PhotoUrl = photo,
                IsPublic = true,
                IsActive = true,
            };

            foreach (var specName in specs)
            {
                lawyer.LawyerSpecialties.Add(new LawyerSpecialty
                {
                    LawyerId = id,
                    SpecialtyId = specialties[specName].Id,
                });
            }

            await _context.Lawyers.AddAsync(lawyer, ct);
        }
    }

    private async Task SeedRoomsAsync(CancellationToken ct)
    {
        var rooms = new[]
        {
            new Room { Id = SeedIds.Room1, SubsectionId = SeedIds.Subsection1, Name = "Sala de Reunião Executive", Description = "Ambiente reservado para reuniões profissionais", Capacity = 6, Features = ["Ar-condicionado", "TV 55\"", "Wi-Fi"], PricePerHour = 40, IsFree = false, Status = RoomStatus.Available, Color = "from-blue-500 to-indigo-600" },
            new Room { Id = SeedIds.Room2, SubsectionId = SeedIds.Subsection1, Name = "Sala de Audiência Formal", Description = "Mesa oval para audiências e mediações", Capacity = 12, Features = ["Mesa oval", "Gravação", "Microfones"], PricePerHour = 70, IsFree = false, Status = RoomStatus.Available, Color = "from-slate-600 to-slate-800" },
            new Room { Id = SeedIds.Room3, SubsectionId = SeedIds.Subsection1, Name = "Coworking Aberto", Description = "Estações compartilhadas para advogados inscritos", Capacity = 20, Features = ["Wi-Fi", "Café incluso", "Estações individuais"], PricePerHour = 0, IsFree = true, Status = RoomStatus.Available, Color = "from-emerald-500 to-teal-600" },
            new Room { Id = SeedIds.Room4, SubsectionId = SeedIds.Subsection1, Name = "Sala de Videoconferência", Description = "Setup completo para audiências online", Capacity = 4, Features = ["Setup pro", "Ring light", "Webcam 4K"], PricePerHour = 35, IsFree = false, Status = RoomStatus.Occupied, Color = "from-rose-500 to-pink-600" },
            new Room { Id = SeedIds.Room5, SubsectionId = SeedIds.Subsection1, Name = "Auditório Principal", Description = "Para eventos, palestras e cursos", Capacity = 80, Features = ["Projetor", "Som profissional", "Palco"], PricePerHour = 200, IsFree = false, Status = RoomStatus.Request, Color = "from-amber-500 to-orange-600" },
        };

        await _context.Rooms.AddRangeAsync(rooms, ct);
    }

    private async Task SeedPostsAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var posts = new[]
        {
            new Post { Id = SeedIds.Post1, PublishedAt = now.AddDays(-2), ImageGradient = "from-blue-400 to-indigo-500", PhotoUrl = "https://images.unsplash.com/photo-1497366754035-f200968a6e72?w=900&q=80&auto=format&fit=crop", Caption = "🎉 Nova sala de coworking inaugurada! Agora temos 3 salas disponíveis para os inscritos. Venha conhecer e reserve a sua.", LikesCount = 47 },
            new Post { Id = SeedIds.Post2, PublishedAt = now.AddDays(-4), ImageGradient = "from-slate-500 to-slate-700", PhotoUrl = "https://images.unsplash.com/photo-1589829545856-d10d557cf95f?w=900&q=80&auto=format&fit=crop", Caption = "📢 Eleições da subseção abertas — confira os candidatos no site oficial e participe da votação.", LikesCount = 89 },
            new Post { Id = SeedIds.Post3, PublishedAt = now.AddDays(-6), ImageGradient = "from-amber-400 to-orange-500", PhotoUrl = "https://images.unsplash.com/photo-1505664194779-8beaceb93744?w=900&q=80&auto=format&fit=crop", Caption = "⚖️ Palestra gratuita: Reforma Trabalhista 2026 — inscreva-se pelo app. Vagas limitadas.", LikesCount = 124 },
            new Post { Id = SeedIds.Post4, PublishedAt = now.AddDays(-7), ImageGradient = "from-emerald-400 to-teal-500", PhotoUrl = "https://images.unsplash.com/photo-1606811971618-4486d14f3f99?w=900&q=80&auto=format&fit=crop", Caption = "🤝 Novo parceiro: Clínica OdontoJur com 30% de desconto para advogados inscritos.", LikesCount = 32 },
            new Post { Id = SeedIds.Post5, PublishedAt = now.AddDays(-14), ImageGradient = "from-rose-400 to-pink-500", PhotoUrl = "https://images.unsplash.com/photo-1450101499163-c8848c66ca85?w=900&q=80&auto=format&fit=crop", Caption = "📋 Prazo: entrega de documentos para a Comissão de Ética até 30/06. Não deixe para a última hora.", LikesCount = 18 },
            new Post { Id = SeedIds.Post6, PublishedAt = now.AddDays(-21), ImageGradient = "from-violet-400 to-purple-500", PhotoUrl = "https://images.unsplash.com/photo-1521737711867-e3b97375f902?w=900&q=80&auto=format&fit=crop", Caption = "✨ Programa de mentoria para advogados recém-formados — inscrições abertas.", LikesCount = 76 },
        };

        foreach (var post in posts)
        {
            post.SubsectionId = SeedIds.Subsection1;
            post.Title = post.Caption.Length > 80 ? post.Caption[..80] : post.Caption;
            post.Source = PostSource.Manual;
            post.IsActive = true;
            post.LinkUrl = null;
        }

        await _context.Posts.AddRangeAsync(posts, ct);
    }

    private async Task SeedSocialAccountsAsync(CancellationToken ct)
    {
        var accounts = new[]
        {
            new SocialAccount
            {
                Id = SeedIds.SocialAccount1,
                SubsectionId = SeedIds.Subsection1,
                Platform = "instagram",
                Handle = "@oabcidadeexemplo",
                ProfileUrl = "https://www.instagram.com/oabcidadeexemplo/",
                Connected = true,
                FollowersCount = 3420,
                LastSync = DateTime.UtcNow.AddHours(-2),
            },
            new SocialAccount
            {
                Id = SeedIds.SocialAccount2,
                SubsectionId = SeedIds.Subsection1,
                Platform = "facebook",
                Handle = "OAB Cidade Exemplo",
                ProfileUrl = "https://www.facebook.com/oabcidadeexemplo/",
                Connected = true,
                FollowersCount = 8150,
                LastSync = DateTime.UtcNow.AddHours(-2),
            },
            new SocialAccount
            {
                Id = SeedIds.SocialAccount3,
                SubsectionId = SeedIds.Subsection1,
                Platform = "linkedin",
                Handle = "OAB Subseção Cidade Exemplo",
                ProfileUrl = "https://linkedin.com/company/oabcidadeexemplo",
                Connected = false,
            },
        };

        await _context.SocialAccounts.AddRangeAsync(accounts, ct);
    }

    private async Task SeedBackofficeUsersAsync(CancellationToken ct)
    {
        var users = new[]
        {
            new AdminUser
            {
                Id = SeedIds.Admin1,
                SubsectionId = SeedIds.Subsection1,
                Name = "Secretaria OAB Cidade Exemplo",
                Email = "admin@subsecao.pro",
                PasswordHash = _passwordHasher.Hash(Environment.GetEnvironmentVariable("SEED_ADMIN_PASSWORD") ?? Guid.NewGuid().ToString("N")),
                Role = "admin",
                IsActive = true,
            },
            new AdminUser
            {
                Id = SeedIds.Admin2,
                SubsectionId = SeedIds.Subsection1,
                Name = "Moderador Hub",
                Email = "moderador@subsecao.pro",
                PasswordHash = _passwordHasher.Hash(Environment.GetEnvironmentVariable("SEED_MODERATOR_PASSWORD") ?? Guid.NewGuid().ToString("N")),
                Role = "moderator",
                IsActive = true,
            },
        };

        await _context.AdminUsers.AddRangeAsync(users, ct);
    }

    private async Task SeedBannersAsync(CancellationToken ct)
    {
        var banners = new[]
        {
            new Banner { Id = SeedIds.Banner1, Title = "Unimed Campinas", Subtitle = "Condições exclusivas para advogados OAB", Cta = "Saiba mais", Color = "from-emerald-700/85 to-green-900/85", PhotoUrl = "https://images.unsplash.com/photo-1505751172876-fa1923c5c528?w=900&q=80&auto=format&fit=crop", LinkTo = "/club/$id", LinkParams = new Dictionary<string, string> { ["id"] = SeedIds.Partner1.ToString() }, SortOrder = 1 },
            new Banner { Id = SeedIds.Banner2, Title = "JuriSpace Coworking", Subtitle = "Reserve seu espaço com 20% off", Cta = "Reservar", Color = "from-blue-700/85 to-indigo-900/85", PhotoUrl = "https://images.unsplash.com/photo-1497366216548-37526070297c?w=900&q=80&auto=format&fit=crop", LinkTo = "/rooms", SortOrder = 2 },
            new Banner { Id = SeedIds.Banner3, Title = "Escola de Direito EAD", Subtitle = "40% off para inscritos OAB", Cta = "Matricular", Color = "from-rose-700/85 to-orange-900/85", PhotoUrl = "https://images.unsplash.com/photo-1523240795612-9a054b0db644?w=900&q=80&auto=format&fit=crop", LinkTo = "/club/$id", LinkParams = new Dictionary<string, string> { ["id"] = SeedIds.Partner3.ToString() }, SortOrder = 3 },
        };

        foreach (var banner in banners)
            banner.SubsectionId = SeedIds.Subsection1;

        await _context.Banners.AddRangeAsync(banners, ct);
    }

    private async Task SeedPartnersAsync(CancellationToken ct)
    {
        var partners = new[]
        {
            new Partner { Id = SeedIds.Partner1, Name = "Unimed Campinas", Category = PartnerCategory.Saude, Discount = "20%", Description = "Desconto no plano empresarial para advogados inscritos.", ValidUntil = new DateOnly(2026, 12, 31), Color = "from-emerald-500 to-green-700" },
            new Partner { Id = SeedIds.Partner2, Name = "OdontoJur Clínica", Category = PartnerCategory.Saude, Discount = "30%", Description = "Procedimentos odontológicos com desconto exclusivo.", ValidUntil = new DateOnly(2026, 12, 31), Color = "from-teal-500 to-cyan-700" },
            new Partner { Id = SeedIds.Partner3, Name = "Escola Damásio", Category = PartnerCategory.Educacao, Discount = "40%", Description = "Pós-graduação em diversas áreas do Direito.", ValidUntil = new DateOnly(2026, 12, 31), Color = "from-blue-500 to-indigo-700" },
            new Partner { Id = SeedIds.Partner4, Name = "LFG Cursos", Category = PartnerCategory.Educacao, Discount = "35%", Description = "Cursos preparatórios para concursos e OAB.", ValidUntil = new DateOnly(2026, 12, 31), Color = "from-indigo-500 to-violet-700" },
            new Partner { Id = SeedIds.Partner5, Name = "Restaurante Escala", Category = PartnerCategory.Alimentacao, Discount = "15%", Description = "Almoço executivo com desconto de segunda a sexta.", ValidUntil = new DateOnly(2026, 12, 31), Color = "from-amber-500 to-orange-600" },
            new Partner { Id = SeedIds.Partner6, Name = "iFood Pro", Category = PartnerCategory.Alimentacao, Discount = "R$20", Description = "Crédito no primeiro pedido para advogados OAB.", ValidUntil = new DateOnly(2026, 12, 31), Color = "from-rose-500 to-red-600" },
            new Partner { Id = SeedIds.Partner7, Name = "Microsoft 365", Category = PartnerCategory.Tecnologia, Discount = "25%", Description = "Desconto no plano anual com benefícios profissionais.", ValidUntil = new DateOnly(2026, 12, 31), Color = "from-sky-500 to-blue-700" },
            new Partner { Id = SeedIds.Partner8, Name = "DocuSign", Category = PartnerCategory.Tecnologia, Discount = "30%", Description = "Assinatura eletrônica com validade jurídica.", ValidUntil = new DateOnly(2026, 12, 31), Color = "from-yellow-500 to-amber-600" },
            new Partner { Id = SeedIds.Partner9, Name = "Cartório Digital", Category = PartnerCategory.Servicos, Discount = "10%", Description = "Reconhecimentos e autenticações online.", ValidUntil = new DateOnly(2026, 12, 31), Color = "from-slate-500 to-slate-700" },
            new Partner { Id = SeedIds.Partner10, Name = "Gráfica JuriPrint", Category = PartnerCategory.Servicos, Discount = "20%", Description = "Impressões, encadernações e materiais jurídicos.", ValidUntil = new DateOnly(2026, 12, 31), Color = "from-violet-500 to-purple-700" },
        };

        foreach (var partner in partners)
            partner.SubsectionId = SeedIds.Subsection1;

        await _context.Partners.AddRangeAsync(partners, ct);
    }

    private async Task SeedNotificationsAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var notifications = new[]
        {
            new Notification { Id = SeedIds.Notification1, Icon = "📅", Text = "Evento: Palestra sobre Direito Digital — 12/06", CreatedAt = now.AddHours(-1), ForLawyerOnly = false, Href = "/about" },
            new Notification { Id = SeedIds.Notification2, Icon = "🔔", Text = "Novo comunicado da diretoria disponível", CreatedAt = now.AddHours(-3), ForLawyerOnly = false, Href = "/about" },
            new Notification { Id = SeedIds.Notification3, Icon = "✅", Text = "Sua reserva da Sala Executive foi confirmada", CreatedAt = now.AddDays(-1), ForLawyerOnly = true, Href = "/reservations" },
        };

        foreach (var notification in notifications)
            notification.SubsectionId = SeedIds.Subsection1;

        await _context.Notifications.AddRangeAsync(notifications, ct);
    }

    private async Task SeedArticlesAsync(CancellationToken ct)
    {
        var articles = new[]
        {
            new Article { Id = SeedIds.Article1, Slug = "consumidor-defeito", Title = "Direitos do Consumidor: o que fazer quando um produto tem defeito", Icon = "🛒", Summary = "Entenda seus direitos e prazos para reclamar de produtos com defeito." },
            new Article { Id = SeedIds.Article2, Slug = "pensao-alimenticia", Title = "Pensão Alimentícia: como funciona e como solicitar", Icon = "👨‍👩‍👧", Summary = "Quem tem direito, como calcular e quais documentos são necessários." },
            new Article { Id = SeedIds.Article3, Slug = "demissao-justa-causa", Title = "Demissão sem Justa Causa: quais são seus direitos", Icon = "💼", Summary = "Verbas rescisórias, FGTS, seguro-desemprego e prazos." },
            new Article { Id = SeedIds.Article4, Slug = "inventario-heranca", Title = "Inventário e Herança: por onde começar", Icon = "📜", Summary = "Documentos, prazos e diferença entre judicial e extrajudicial." },
            new Article { Id = SeedIds.Article5, Slug = "violencia-domestica", Title = "Violência Doméstica: onde buscar ajuda jurídica", Icon = "🛡️", Summary = "Medidas protetivas, Lei Maria da Penha e atendimento gratuito." },
            new Article { Id = SeedIds.Article6, Slug = "inquilino-proprietario", Title = "Inquilino vs Proprietário: direitos na locação de imóvel", Icon = "🏠", Summary = "Reajustes, vistorias, multas e rescisão de contrato." },
        };

        foreach (var article in articles)
            article.SubsectionId = SeedIds.Subsection1;

        await _context.Articles.AddRangeAsync(articles, ct);
    }

    private async Task SeedReservationsAsync(CancellationToken ct)
    {
        var reservations = new[]
        {
            new Reservation { Id = SeedIds.Reservation1, LawyerId = SeedIds.Lawyer8, RoomId = SeedIds.Room1, Date = new DateOnly(2026, 5, 12), StartTime = new TimeOnly(14, 0), EndTime = new TimeOnly(16, 0), Status = ReservationStatus.Confirmed, Amount = 83.2m },
            new Reservation { Id = SeedIds.Reservation2, LawyerId = SeedIds.Lawyer8, RoomId = SeedIds.Room4, Date = new DateOnly(2026, 4, 28), StartTime = new TimeOnly(10, 0), EndTime = new TimeOnly(11, 0), Status = ReservationStatus.Confirmed, Amount = 36.4m },
            new Reservation { Id = SeedIds.Reservation3, LawyerId = SeedIds.Lawyer8, RoomId = SeedIds.Room3, Date = new DateOnly(2026, 4, 15), StartTime = new TimeOnly(9, 0), EndTime = new TimeOnly(13, 0), Status = ReservationStatus.Confirmed, Amount = 0m },
            new Reservation { Id = SeedIds.Reservation4, LawyerId = SeedIds.Lawyer8, RoomId = SeedIds.Room1, Date = new DateOnly(2026, 6, 2), StartTime = new TimeOnly(15, 0), EndTime = new TimeOnly(17, 0), Status = ReservationStatus.Pending, Amount = 83.2m },
        };

        foreach (var reservation in reservations)
            reservation.SubsectionId = SeedIds.Subsection1;

        await _context.Reservations.AddRangeAsync(reservations, ct);
    }

    private async Task SeedElectionAsync(CancellationToken ct)
    {
        var election = new Election
        {
            Id = SeedIds.Election1,
            SubsectionId = SeedIds.Subsection1,
            Year = 2026,
            Term = "2026–2028",
            EndDate = new DateOnly(2026, 6, 30),
            PortalUrl = "votacao.oab-cidade-exemplo.org.br/2026",
            IsActive = true,
            Candidates =
            [
                new Candidate
                {
                    Id = SeedIds.Candidate1,
                    ElectionId = SeedIds.Election1,
                    Name = "Dra. Helena Ribeiro",
                    Slate = "Chapa 1 — Renovação OAB",
                    Number = "10",
                    Bio = "15 anos como advogada civil. Foco em modernização e transparência.",
                    ColorStart = "#3b82f6",
                    ColorEnd = "#4338ca",
                },
                new Candidate
                {
                    Id = SeedIds.Candidate2,
                    ElectionId = SeedIds.Election1,
                    Name = "Dr. Paulo Andrade",
                    Slate = "Chapa 2 — Tradição e Diálogo",
                    Number = "20",
                    Bio = "Ex-conselheiro estadual. Defende valorização da advocacia local.",
                    ColorStart = "#10b981",
                    ColorEnd = "#0f766e",
                },
                new Candidate
                {
                    Id = SeedIds.Candidate3,
                    ElectionId = SeedIds.Election1,
                    Name = "Dra. Mariana Soares",
                    Slate = "Chapa 3 — Advocacia Independente",
                    Number = "30",
                    Bio = "Especialista em direito digital. Pauta jovem e tecnologia.",
                    ColorStart = "#f43f5e",
                    ColorEnd = "#be185d",
                },
            ],
        };

        await _context.Elections.AddAsync(election, ct);
    }

    private async Task EnsureElectionAsync(CancellationToken ct)
    {
        if (!await _context.Elections.AnyAsync(ct))
        {
            await SeedElectionAsync(ct);
            await _context.SaveChangesAsync(ct);
        }
    }

    private async Task EnsureBackofficeUsersAsync(CancellationToken ct)
    {
        var expected = new[]
        {
            new { Id = SeedIds.Admin1, Name = "Secretaria OAB Cidade Exemplo", Email = "admin@subsecao.pro", Role = "admin", Password = Environment.GetEnvironmentVariable("SEED_ADMIN_PASSWORD") ?? Guid.NewGuid().ToString("N") },
            new { Id = SeedIds.Admin2, Name = "Moderador Hub", Email = "moderador@subsecao.pro", Role = "moderator", Password = Environment.GetEnvironmentVariable("SEED_MODERATOR_PASSWORD") ?? Guid.NewGuid().ToString("N") },
        };

        foreach (var user in expected)
        {
            var existing = await _context.AdminUsers.FirstOrDefaultAsync(a => a.Email == user.Email, ct);
            if (existing is null)
            {
                await _context.AdminUsers.AddAsync(new AdminUser
                {
                    Id = user.Id,
                    SubsectionId = SeedIds.Subsection1,
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role,
                    IsActive = true,
                    PasswordHash = _passwordHasher.Hash(user.Password),
                }, ct);
                continue;
            }

            existing.Name = user.Name;
            existing.Role = user.Role;
            existing.IsActive = true;
        }

        await _context.SaveChangesAsync(ct);
    }

    private async Task EnsureSocialAccountsAsync(CancellationToken ct)
    {
        if (await _context.SocialAccounts.AnyAsync(ct)) return;
        await SeedSocialAccountsAsync(ct);
        await _context.SaveChangesAsync(ct);
    }
}
