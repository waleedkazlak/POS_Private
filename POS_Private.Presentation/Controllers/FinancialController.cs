using POS_Private.Application.Commands.Financial;
using POS_Private.Application.DTOs;
using POS_Private.Application.Queries.Financial;
using POS_Private.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace POS_Private.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FinancialController : ControllerBase
{
    private readonly IMediator _mediator;

    public FinancialController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // ==========================================
    // Chart of Accounts (COA)
    // ==========================================

    [HttpGet("accounts")]
    [ProducesResponseType(typeof(APIBaseResponse<IEnumerable<AccountDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccounts([FromQuery] bool? isActive, [FromQuery] AccountType? type)
    {
        var response = await _mediator.Send(new GetAccountsQuery(isActive, type));
        return Ok(response);
    }

    [HttpGet("accounts/hierarchy")]
    [ProducesResponseType(typeof(APIBaseResponse<IEnumerable<AccountDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccountHierarchy()
    {
        var response = await _mediator.Send(new GetAccountHierarchyQuery());
        return Ok(response);
    }

    [HttpGet("accounts/{id:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<AccountDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccountById(int id)
    {
        var response = await _mediator.Send(new GetAccountByIdQuery(id));
        return Ok(response);
    }

    [HttpPost("accounts")]
    [ProducesResponseType(typeof(APIBaseResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto dto)
    {
        var response = await _mediator.Send(new CreateAccountCommand(dto));
        return Ok(response);
    }

    [HttpPut("accounts/{id:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateAccount(int id, [FromBody] UpdateAccountDto dto)
    {
        var response = await _mediator.Send(new UpdateAccountCommand(id, dto));
        return Ok(response);
    }

    [HttpDelete("accounts/{id:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteAccount(int id)
    {
        var response = await _mediator.Send(new DeleteAccountCommand(id));
        return Ok(response);
    }

    // ==========================================
    // Cost Centers
    // ==========================================

    [HttpGet("cost-centers")]
    [ProducesResponseType(typeof(APIBaseResponse<IEnumerable<CostCenterDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCostCenters([FromQuery] bool? isActive, [FromQuery] int? branchId)
    {
        var response = await _mediator.Send(new GetCostCentersQuery(isActive, branchId));
        return Ok(response);
    }

    [HttpGet("cost-centers/{id:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<CostCenterDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCostCenterById(int id)
    {
        var response = await _mediator.Send(new GetCostCenterByIdQuery(id));
        return Ok(response);
    }

    [HttpPost("cost-centers")]
    [ProducesResponseType(typeof(APIBaseResponse<int>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateCostCenter([FromBody] CreateCostCenterDto dto)
    {
        var response = await _mediator.Send(new CreateCostCenterCommand(dto));
        return Ok(response);
    }

    [HttpPut("cost-centers/{id:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateCostCenter(int id, [FromBody] UpdateCostCenterDto dto)
    {
        var response = await _mediator.Send(new UpdateCostCenterCommand(id, dto));
        return Ok(response);
    }

    [HttpDelete("cost-centers/{id:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteCostCenter(int id)
    {
        var response = await _mediator.Send(new DeleteCostCenterCommand(id));
        return Ok(response);
    }

    // ==========================================
    // Journal Entries (GL)
    // ==========================================

    [HttpGet("journal-entries")]
    [ProducesResponseType(typeof(APIBaseResponse<PaginatedResultDto<JournalEntryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetJournalEntries(
        [FromQuery] string? searchTerm,
        [FromQuery] JournalEntryStatus? status,
        [FromQuery] JournalSourceType? sourceType,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int? accountId,
        [FromQuery] int? costCenterId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20)
    {
        var response = await _mediator.Send(new GetJournalEntriesQuery(
            searchTerm, status, sourceType, fromDate, toDate, accountId, costCenterId, pageNumber, pageSize));
        return Ok(response);
    }

    [HttpGet("journal-entries/{id:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<JournalEntryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetJournalEntryById(int id)
    {
        var response = await _mediator.Send(new GetJournalEntryByIdQuery(id));
        return Ok(response);
    }

    [HttpPost("journal-entries")]
    [ProducesResponseType(typeof(APIBaseResponse<JournalEntryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateJournalEntry([FromBody] CreateJournalEntryDto dto)
    {
        var response = await _mediator.Send(new CreateJournalEntryCommand(dto));
        return Ok(response);
    }

    [HttpPost("journal-entries/{id:int}/post")]
    [ProducesResponseType(typeof(APIBaseResponse<JournalEntryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> PostJournalEntry(int id)
    {
        var response = await _mediator.Send(new PostJournalEntryCommand(id));
        return Ok(response);
    }

    [HttpPost("journal-entries/{id:int}/void")]
    [ProducesResponseType(typeof(APIBaseResponse<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> VoidJournalEntry(int id, [FromBody] VoidJournalEntryDto dto)
    {
        var response = await _mediator.Send(new VoidJournalEntryCommand(id, dto));
        return Ok(response);
    }

    [HttpPost("journal-entries/{id:int}/eta-submit")]
    [ProducesResponseType(typeof(APIBaseResponse<JournalEntryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SubmitToETA(int id)
    {
        var response = await _mediator.Send(new SubmitJournalEntryToETACommand(id));
        return Ok(response);
    }

    // ==========================================
    // Financial Reports & Accountant Audit
    // ==========================================

    [HttpGet("reports/trial-balance")]
    [ProducesResponseType(typeof(APIBaseResponse<TrialBalanceReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTrialBalance(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int? costCenterId)
    {
        var response = await _mediator.Send(new GetTrialBalanceQuery(fromDate, toDate, costCenterId));
        return Ok(response);
    }

    [HttpGet("reports/account-ledger/{accountId:int}")]
    [ProducesResponseType(typeof(APIBaseResponse<AccountLedgerReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAccountLedger(
        int accountId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int? costCenterId)
    {
        var response = await _mediator.Send(new GetAccountLedgerQuery(accountId, fromDate, toDate, costCenterId));
        return Ok(response);
    }

    [HttpGet("reports/cost-center-income-statement")]
    [ProducesResponseType(typeof(APIBaseResponse<CostCenterIncomeStatementDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCostCenterIncomeStatement(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        var response = await _mediator.Send(new GetCostCenterIncomeStatementQuery(fromDate, toDate));
        return Ok(response);
    }

    [HttpGet("reports/eta-tax-summary")]
    [ProducesResponseType(typeof(APIBaseResponse<ETATaxSummaryReportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetETATaxSummary(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        var response = await _mediator.Send(new GetETATaxSummaryQuery(fromDate, toDate));
        return Ok(response);
    }

    // ==========================================
    // ETA Company Settings
    // ==========================================

    [HttpGet("eta-settings")]
    [ProducesResponseType(typeof(APIBaseResponse<ETACompanySettingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetETASettings()
    {
        var response = await _mediator.Send(new GetETACompanySettingQuery());
        return Ok(response);
    }

    [HttpPut("eta-settings")]
    [ProducesResponseType(typeof(APIBaseResponse<ETACompanySettingDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateETASettings([FromBody] UpdateETACompanySettingDto dto)
    {
        var response = await _mediator.Send(new UpdateETACompanySettingCommand(dto));
        return Ok(response);
    }
}
