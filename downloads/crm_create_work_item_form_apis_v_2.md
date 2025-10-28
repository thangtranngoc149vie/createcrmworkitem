# CRM — Create Work Item Form APIs v1.0

**Base URL:** `/api/v1/crm`
**Auth:** `Authorization: Bearer <JWT>`
**Stack:** .NET 8 + Dapper + PostgreSQL (schema v3.1c)

---

## 0) UI → API mapping (tóm tắt)
| UI Control | Sự kiện | API |
|---|---|---|
| Category (cấp 1) | On focus / Domain change | `GET /wi-categories?level=1&domain={domain}&q=` |
| Subcategory | Khi chọn Category | `GET /wi-categories/{id}/children?q=` |
| Assignee / Watchers (autocomplete) | On type | `GET /users/search?q=` |
| Scope entity (project/department/user) | On type | `GET /scope/search?type=...&q=` |
| Template gợi ý | Khi đủ type/domain/category/sub | `GET /templates/suggest?...` |
| Submit | On submit | `POST /work-items` |
| Redirect chi tiết | After create | `GET /work-items/{id}` |

---

## 1) OpenAPI 3.0 YAML
```yaml
openapi: 3.0.3
info:
  title: CRM — Create Work Item Form APIs
  version: 1.0.0
  description: |
    API phục vụ màn hình **Tạo Yêu Cầu** (ticket/request/approval) trong CRM.
    Bao gồm: danh mục, autocomplete người dùng/phạm vi, gợi ý template, tạo work item, và xem chi tiết.
servers:
  - url: /api/v1/crm
security:
  - bearerAuth: []

paths:
  /wi-categories:
    get:
      summary: Lấy danh mục cấp 1 (Category)
      operationId: listRootCategories
      parameters:
        - in: query
          name: level
          schema: { type: integer, default: 1, minimum: 1 }
          description: Cấp danh mục (mặc định 1 = gốc)
        - in: query
          name: domain
          schema: { type: string }
          description: Lọc theo domain nghiệp vụ (nếu áp dụng)
        - in: query
          name: q
          schema: { type: string }
          description: Tìm theo tên (không dấu)
        - $ref: '#/components/parameters/page'
        - $ref: '#/components/parameters/page_size'
      responses:
        '200':
          description: Danh sách category gốc
          content:
            application/json:
              schema:
                type: object
                properties:
                  items:
                    type: array
                    items: { $ref: '#/components/schemas/Category' }
                  total: { type: integer }
                  page: { type: integer }
                  page_size: { type: integer }
        '401': { $ref: '#/components/responses/Unauthorized' }
        '500': { $ref: '#/components/responses/ServerError' }

  /wi-categories/{categoryId}/children:
    get:
      summary: Lấy subcategory theo category cha
      operationId: listSubcategories
      parameters:
        - in: path
          name: categoryId
          required: true
          schema: { type: string, format: uuid }
        - in: query
          name: q
          schema: { type: string }
          description: Tìm theo tên (không dấu)
      responses:
        '200':
          description: Danh sách subcategory
          content:
            application/json:
              schema:
                type: object
                properties:
                  items:
                    type: array
                    items: { $ref: '#/components/schemas/Subcategory' }
        '401': { $ref: '#/components/responses/Unauthorized' }
        '404': { $ref: '#/components/responses/NotFound' }
        '500': { $ref: '#/components/responses/ServerError' }

  /users/search:
    get:
      summary: Autocomplete người dùng (assignee/watchers)
      operationId: searchUsers
      parameters:
        - in: query
          name: q
          schema: { type: string }
          description: Tên/email (không dấu)
        - in: query
          name: limit
          schema: { type: integer, default: 10, minimum: 1, maximum: 50 }
      responses:
        '200':
          description: Kết quả người dùng
          content:
            application/json:
              schema:
                type: object
                properties:
                  items:
                    type: array
                    items: { $ref: '#/components/schemas/UserSummary' }
        '401': { $ref: '#/components/responses/Unauthorized' }
        '500': { $ref: '#/components/responses/ServerError' }

  /scope/search:
    get:
      summary: Autocomplete phạm vi quyền (project/department/user)
      operationId: searchScope
      parameters:
        - in: query
          name: type
          required: true
          schema:
            type: string
            enum: [project, department, user]
        - in: query
          name: q
          schema: { type: string }
        - in: query
          name: limit
          schema: { type: integer, default: 10, minimum: 1, maximum: 50 }
      responses:
        '200':
          description: Kết quả phạm vi
          content:
            application/json:
              schema:
                type: object
                properties:
                  items:
                    type: array
                    items: { $ref: '#/components/schemas/ScopeItem' }
        '400': { $ref: '#/components/responses/BadRequest' }
        '401': { $ref: '#/components/responses/Unauthorized' }
        '500': { $ref: '#/components/responses/ServerError' }

  /templates/suggest:
    get:
      summary: Gợi ý template & defaults
      operationId: suggestTemplate
      parameters:
        - in: query
          name: type
          required: true
          schema:
            type: string
            enum: [request, ticket, approval]
        - in: query
          name: domain
          schema: { type: string }
        - in: query
          name: category
          schema: { type: string, format: uuid }
        - in: query
          name: subcategory
          schema: { type: string, format: uuid }
      responses:
        '200':
          description: Gợi ý áp dụng
          content:
            application/json:
              schema: { $ref: '#/components/schemas/TemplateSuggestResponse' }
        '401': { $ref: '#/components/responses/Unauthorized' }
        '500': { $ref: '#/components/responses/ServerError' }

  /work-items:
    post:
      summary: Tạo work item (request/ticket/approval)
      operationId: createWorkItem
      requestBody:
        required: true
        content:
          application/json:
            schema: { $ref: '#/components/schemas/WorkItemCreateRequest' }
            example:
              type: request
              domain: admin
              category_id: c2
              subcategory_id: s1
              title: "Đề nghị đặt phòng họp 14h-16h"
              description: "Cuộc họp dự án A..."
              priority: medium
              due_at: "2025-10-28T09:00:00+07:00"
              scope: { type: department, entity_id: d1 }
              assignee_id: u2
              watcher_ids: [u3, u4]
      responses:
        '201':
          description: Tạo thành công
          content:
            application/json:
              schema:
                type: object
                properties:
                  id: { type: string, format: uuid }
                  ref_code: { type: string }
                  status: { type: string }
                  created_at: { type: string, format: date-time }
        '400': { $ref: '#/components/responses/BadRequest' }
        '401': { $ref: '#/components/responses/Unauthorized' }
        '409': { $ref: '#/components/responses/Conflict' }
        '500': { $ref: '#/components/responses/ServerError' }
      security:
        - bearerAuth: []
      parameters:
        - in: header
          name: Idempotency-Key
          schema: { type: string }
          required: false
          description: Khuyến nghị để chống double-submit

  /work-items/{id}:
    get:
      summary: Lấy chi tiết work item sau khi tạo
      operationId: getWorkItem
      parameters:
        - in: path
          name: id
          required: true
          schema: { type: string, format: uuid }
      responses:
        '200':
          description: Chi tiết work item
          content:
            application/json:
              schema: { $ref: '#/components/schemas/WorkItemResponse' }
        '401': { $ref: '#/components/responses/Unauthorized' }
        '404': { $ref: '#/components/responses/NotFound' }
        '500': { $ref: '#/components/responses/ServerError' }

components:
  securitySchemes:
    bearerAuth:
      type: http
      scheme: bearer
      bearerFormat: JWT

  parameters:
    page:
      in: query
      name: page
      schema: { type: integer, default: 1, minimum: 1 }
      description: Trang hiện tại
    page_size:
      in: query
      name: page_size
      schema: { type: integer, default: 20, minimum: 1, maximum: 100 }
      description: Kích thước trang

  responses:
    Unauthorized:
      description: Chưa xác thực
      content:
        application/json:
          schema: { $ref: '#/components/schemas/Error' }
    BadRequest:
      description: Tham số không hợp lệ
      content:
        application/json:
          schema: { $ref: '#/components/schemas/Error' }
    NotFound:
      description: Không tìm thấy
      content:
        application/json:
          schema: { $ref: '#/components/schemas/Error' }
    Conflict:
      description: Xung đột (idempotency hoặc trạng thái)
      content:
        application/json:
          schema: { $ref: '#/components/schemas/Error' }
    ServerError:
      description: Lỗi hệ thống
      content:
        application/json:
          schema: { $ref: '#/components/schemas/Error' }

  schemas:
    Category:
      type: object
      properties:
        id: { type: string }
        code: { type: string }
        name: { type: string }
        children_count: { type: integer }
    Subcategory:
      type: object
      properties:
        id: { type: string }
        code: { type: string }
        name: { type: string }
    UserSummary:
      type: object
      properties:
        id: { type: string }
        full_name: { type: string }
        email: { type: string }
    ScopeItem:
      type: object
      properties:
        id: { type: string }
        code: { type: string, nullable: true }
        name: { type: string }
    TemplateSuggestResponse:
      type: object
      properties:
        template_code: { type: string }
        assignment:
          type: object
          properties:
            mode: { type: string, enum: [user, role, queue] }
            assignee_id: { type: string, nullable: true }
            display: { type: string, nullable: true }
        scope:
          type: object
          properties:
            type: { type: string, enum: [project, department, user], nullable: true }
            entity_id: { type: string, nullable: true }
            display: { type: string, nullable: true }
        due_in_days: { type: integer, nullable: true }
    WorkItemCreateRequest:
      type: object
      required: [type, title]
      properties:
        type:
          type: string
          enum: [request, ticket, approval]
        domain: { type: string, nullable: true }
        category_id: { type: string, nullable: true }
        subcategory_id: { type: string, nullable: true }
        title: { type: string }
        description: { type: string, nullable: true }
        priority: { type: string, enum: [low, medium, high], default: medium }
        due_at: { type: string, format: date-time, nullable: true }
        scope:
          type: object
          nullable: true
          properties:
            type: { type: string, enum: [project, department, user] }
            entity_id: { type: string }
        assignee_id: { type: string, nullable: true }
        watcher_ids:
          type: array
          items: { type: string }
    WorkItemResponse:
      type: object
      properties:
        id: { type: string }
        ref_code: { type: string }
        ref_type: { type: string }
        title: { type: string }
        status: { type: string }
        priority: { type: string }
        category:
          type: object
          properties:
            id: { type: string }
            name: { type: string }
        subcategory:
          type: object
          properties:
            id: { type: string }
            name: { type: string }
        assignee:
          type: object
          nullable: true
          properties:
            id: { type: string }
            full_name: { type: string }
        watchers:
          type: array
          items:
            type: object
            properties:
              id: { type: string }
              full_name: { type: string }
        due_at: { type: string, format: date-time, nullable: true }
        created_at: { type: string, format: date-time }
    Error:
      type: object
      properties:
        error: { type: string }
        message: { type: string }
        fields:
          type: object
          additionalProperties: { type: string }
        trace_id: { type: string }
```

---

## 2) Authorization & RBAC (bổ sung)

### 2.1 Mô hình hiện tại
- **JWT**
  - Validate: `IssuerSigningKey`, `ValidateIssuer = false`, `ValidateAudience = false`, `ClockSkew = 0`.
  - Claims khi login: `NameIdentifier` (user_id), `Name` (username), `Email`, `RoleId`, `OrgId`.
- **Controller**
  - Dùng `[Authorize]` để yêu cầu token.
  - Lấy claim trong action: `User.FindFirstValue(ClaimTypes.NameIdentifier)`/`User.FindFirst("RoleId")?.Value`/`User.FindFirst("OrgId")?.Value`.
- **Service-level RBAC**
  - Service nhận `user_id`, `role_id`, `org_id` và danh sách permission cần kiểm tra.
  - Đọc cấu hình quyền từ `appsettings.json` (ví dụ: `Permission:ProjectDocumentsCreate`, `Permission:ProjectDocumentsAdmin`).
  - Gọi `_RoleRepository.RbacCheckHasOnePermissionsInListAsync(user_id, listPermissions)`; nếu **không** có ít nhất một quyền -> trả `401 Unauthorized`.

### 2.2 Chuẩn hoá quyền cho màn hình **Create Work Item** (CRM)
> Gợi ý đặt key permission theo module `crm` để phân tách với `proj`.

| Endpoint | Ý nghĩa | Permission yêu cầu (Một trong) |
|---|---|---|
| `GET /wi-categories` | Đọc danh mục | `crm:cat:read` |
| `GET /wi-categories/{id}/children` | Đọc subcategory | `crm:cat:read` |
| `GET /users/search` | Tìm user (assignee/watchers) | `crm:user:read`, `crm:directory:read` |
| `GET /scope/search?type=project` | Tìm project cho scope | `crm:scope:project:read` |
| `GET /scope/search?type=department` | Tìm phòng ban | `crm:scope:department:read` |
| `GET /scope/search?type=user` | Tìm người dùng cho scope | `crm:scope:user:read` |
| `GET /templates/suggest` | Gợi ý template | `crm:template:suggest:read` |
| `POST /work-items` | Tạo yêu cầu | `crm:wi:create`, `crm:wi:admin` |
| `GET /work-items/{id}` | Xem chi tiết sau khi tạo | `crm:wi:read`, `crm:wi:admin` |

> Tuỳ tổ chức, có thể gộp `crm:scope:*` thành `crm:scope:read`, hoặc dùng ABAC theo `OrgId`/`DepartmentId`.

### 2.3 `appsettings.json` (ví dụ)
```json
{
  "Permission": {
    "CrmCategoryRead": "crm:cat:read",
    "CrmUserRead": "crm:user:read",
    "CrmScopeProjectRead": "crm:scope:project:read",
    "CrmScopeDepartmentRead": "crm:scope:department:read",
    "CrmScopeUserRead": "crm:scope:user:read",
    "CrmTemplateSuggestRead": "crm:template:suggest:read",
    "CrmWorkItemCreate": "crm:wi:create",
    "CrmWorkItemAdmin": "crm:wi:admin",
    "CrmWorkItemRead": "crm:wi:read"
  }
}
```

### 2.4 Controller pattern (rút gọn)
```csharp
[Authorize]
[HttpPost("/api/v1/crm/work-items")]
public async Task<IActionResult> CreateAsync([FromBody] WorkItemCreateRequest dto)
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var roleId = User.FindFirst("RoleId")?.Value;
    var orgId  = User.FindFirst("OrgId")?.Value;
    if (userId is null || orgId is null) return Unauthorized();

    var perms = new List<string>
    {
        _cfg["Permission:CrmWorkItemCreate"],
        _cfg["Permission:CrmWorkItemAdmin"],
    };

    var has = await _roleRepo.RbacCheckHasOnePermissionsInListAsync(Guid.Parse(userId), perms);
    if (!has) return Unauthorized(new{ error="unauthorized", message="RBAC denied"});

    var result = await _svc.CreateAsync(Guid.Parse(userId), Guid.Parse(orgId), dto);
    return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Id }, result);
}
```

### 2.5 Vendor extension trong OpenAPI (khuyến nghị)
Có thể thêm `x-permissions` vào từng operation để FE/QA nhìn thấy ma trận quyền trực tiếp trong spec.

```yaml
paths:
  /work-items:
    post:
      summary: Tạo work item
      x-permissions: [crm:wi:create, crm:wi:admin]
  /wi-categories:
    get:
      x-permissions: [crm:cat:read]
  /wi-categories/{categoryId}/children:
    get:
      x-permissions: [crm:cat:read]
  /users/search:
    get:
      x-permissions: [crm:user:read]
  /scope/search:
    get:
      x-permissions: [crm:scope:project:read, crm:scope:department:read, crm:scope:user:read]
  /templates/suggest:
    get:
      x-permissions: [crm:template:suggest:read]
  /work-items/{id}:
    get:
      x-permissions: [crm:wi:read, crm:wi:admin]
```

### 2.6 Ràng buộc & kiểm tra bổ sung
- **Org boundary**: mọi truy vấn phải filter theo `OrgId` từ claim.
- **Ownership/Scope**: khi `scope.type = department/user`, kiểm tra người gọi có quyền truy cập phạm vi đó.
- **Audit**: log Serilog `user_id`, `org_id`, `role_id`, `permissions_checked` cho mỗi call.
- **Outbox & SignalR**: chỉ phát sự kiện nếu RBAC pass.

---

## 3) Ghi chú triển khai
- Áp dụng Serilog, RBAC (middleware), kiểm tra `org_id`/scope.
- Dapper dùng transaction cho `POST /work-items` (insert item → watchers → outbox → optional workflow instance).
- Hỗ trợ `Idempotency-Key` header để tránh double-submit.
- Search dùng `unaccent` + index GIN/trigram cho tên không dấu.

