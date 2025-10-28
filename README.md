# CRM Create Work Item APIs

Dự án này triển khai đầy đủ các API phục vụ màn hình **Tạo Work Item** trong CRM theo đặc tả `crm_create_work_item_form_apis_v_2.md`. Ứng dụng được xây dựng trên nền tảng ASP.NET Core 8, sử dụng Dapper để làm việc với cơ sở dữ liệu PostgreSQL và xác thực bằng JWT.

## Cấu trúc chính

- `src/CreateCrmWorkItem.Api`: Web API project với các controller:
  - `WiCategoriesController`: lấy danh mục và danh mục con.
  - `UsersController`: autocomplete người dùng.
  - `ScopeController`: tìm phạm vi project/department/user.
  - `TemplatesController`: gợi ý template theo ngữ cảnh.
  - `WorkItemsController`: tạo work item và xem chi tiết.
- `Infrastructure`: cấu hình kết nối PostgreSQL, dịch vụ RBAC, kiểm tra quyền.
- `Services`: lớp nghiệp vụ thao tác dữ liệu bằng Dapper.

## Chạy ứng dụng

1. Cập nhật chuỗi kết nối PostgreSQL trong `appsettings.json` (`ConnectionStrings:CrmDatabase`).
2. Đảm bảo có cấu trúc bảng / view tương ứng (ví dụ `crm_categories`, `crm_subcategories`, `crm_users`, `crm_work_items`, `crm_work_item_watchers`, `crm_work_item_idempotency`, ... như mô tả trong đặc tả).
3. Cài đặt .NET 8 SDK.
4. Khởi chạy API:
   ```bash
   dotnet run --project src/CreateCrmWorkItem.Api/CreateCrmWorkItem.Api.csproj
   ```
5. Sử dụng JWT hợp lệ (header `Authorization: Bearer <token>`). Các tuỳ chọn xác thực có thể cấu hình trong phần `Jwt` của `appsettings.json`.

## RBAC và quyền hạn

- Mỗi endpoint sẽ kiểm tra quyền theo cấu hình ở `Permission` (ví dụ `crm:wi:create`, `crm:cat:read`, ...).
- Hệ thống gọi `_RoleRepository` (Dapper) để kiểm tra người dùng có ít nhất một quyền yêu cầu.
- Có thể tinh chỉnh chuỗi quyền trong `appsettings.json` để phù hợp tổ chức.

## API nổi bật

| Endpoint | Mô tả | Quyền yêu cầu |
| --- | --- | --- |
| `GET /api/v1/crm/wi-categories` | Danh sách danh mục cấp 1, hỗ trợ phân trang & tìm kiếm | `crm:cat:read` |
| `GET /api/v1/crm/wi-categories/{id}/children` | Danh sách danh mục con | `crm:cat:read` |
| `GET /api/v1/crm/users/search` | Autocomplete assignee/watchers | `crm:user:read` hoặc `crm:directory:read` |
| `GET /api/v1/crm/scope/search?type=...` | Tra cứu project/department/user cho scope | `crm:scope:project:read` / `crm:scope:department:read` / `crm:scope:user:read` |
| `GET /api/v1/crm/templates/suggest` | Gợi ý template, assignee, scope mặc định | `crm:template:suggest:read` |
| `POST /api/v1/crm/work-items` | Tạo work item mới (hỗ trợ Idempotency-Key) | `crm:wi:create` hoặc `crm:wi:admin` |
| `GET /api/v1/crm/work-items/{id}` | Lấy chi tiết work item | `crm:wi:read` hoặc `crm:wi:admin` |

## Ghi chú triển khai

- Middleware `ExceptionHandlingMiddleware` chuẩn hoá lỗi theo format `{ error, message, fields, traceId }`.
- Dapper được sử dụng với transaction để đảm bảo tạo work item và watchers là nguyên tử.
- Bảng `crm_work_item_idempotency` lưu kết quả để chống double-submit khi client truyền `Idempotency-Key`.
- Swagger UI có sẵn ở môi trường Development tại `/swagger` để tiện kiểm thử.
