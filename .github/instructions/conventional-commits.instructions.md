---
applyTo: '*'
---
# Conventional Commits Instructions

Adopt the [Conventional Commits](https://www.conventionalcommits.org/) specification for all commit messages to ensure a readable history, automate changelog generation, and facilitate continuous integration.

## Main Rules

- The commit message must be structured as follows:
  ```
  <type>[optional scope]: <description>
  ```
  - **type**: `feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `build`, `ci`, `chore`, `revert`
  - **scope** (optional): the part of the code concerned (e.g., `api`, `domain`, `infrastructure`, `tests`)
  - **description**: short imperative description, no initial capital letter, no period at the end
  - **first line must not exceed 72 characters**

- Examples:
  - `feat(api): add order endpoint`
  - `fix(domain): correct order validation logic`
  - `test(order): add unit tests for order creation`
  - `chore: update dependencies`

## Best Practices

- Use English for all commit messages.
- One commit = one logical/unit change.
- Use the scope to specify the affected layer or feature.
- For breaking changes, add `!` after the type or scope and detail in the commit body.

---

---

## 🌿 Branch Naming Rules

Branch names must follow a simplified Conventional Commits structure:

```
<type>/<short-description-with-hyphens>
```

### **Allowed Types**

Same as commit types:
`feat`, `fix`, `docs`, `style`, `refactor`, `perf`, `test`, `build`, `ci`, `chore`, `revert`

### **Guidelines**

* Use lowercase letters only.
* Use `-` instead of spaces.
* Keep it concise and descriptive.
* Avoid special characters or uppercase letters.

### ✅ Examples

| Type       | Example Branch Name           | Purpose                      |
| ---------- | ----------------------------- | ---------------------------- |
| `feat`     | `feat/add-user-login`         | Add new login feature        |
| `fix`      | `fix/order-total-calculation` | Fix order total bug          |
| `docs`     | `docs/update-readme`          | Update README file           |
| `refactor` | `refactor/payment-service`    | Refactor payment service     |
| `test`     | `test/add-api-tests`          | Add API test cases           |
| `chore`    | `chore/update-dependencies`   | Maintenance or build updates |

### 🚫 Invalid Examples

| ❌ Invalid             | ✅ Correct             |
| --------------------- | --------------------- |
| `feature/add-login`   | `feat/add-login`      |
| `Fix-Bug`             | `fix/bug`             |
| `Feat/AddUser`        | `feat/add-user`       |
| `refactor_login_flow` | `refactor/login-flow` |

Follow this convention for all project commits.
