# Security Policy

## Supported Versions

Lucky Penny Software LLC maintains the latest major versions of its libraries.

Security updates are provided for:
- The current major version
- The previous major version

Versions outside of active maintenance may not receive security updates.

---

## Reporting a Vulnerability

If you believe you have identified a security vulnerability, please report it using GitHub’s private vulnerability reporting feature:

👉 Use the "Report a vulnerability" option in the Security tab of this repository.

This ensures reports are handled securely and privately.

If GitHub reporting is not available, you may contact:

📧 security@luckypennysoftware.com

Please include:
- A description of the vulnerability
- Steps to reproduce (if applicable)
- Potential impact

---

## Disclosure Process

Lucky Penny Software LLC follows a responsible disclosure process:

1. Vulnerabilities are reviewed and validated
2. Fixes are developed and tested
3. Updates are released via new package versions (e.g., NuGet)
4. Public disclosure is made via GitHub Security Advisories when appropriate

We aim to address critical vulnerabilities as quickly as practicable.

---

## Code Signing

Lucky Penny Software LLC signs distributed packages using a trusted code signing certificate.

This ensures:
- Authenticity of published packages
- Integrity of package contents
- Protection against tampering during distribution

Signed packages can be verified by consumers using standard tooling.

---

## Security Practices

Lucky Penny Software LLC develops distributed software libraries and does not operate a hosted service or process customer or financial data.

Security practices include:
- Dependency monitoring and vulnerability alerts (e.g., GitHub Dependabot)
- Review of security advisories and upstream disclosures
- Timely remediation of identified vulnerabilities
- Secure development practices and code review prior to release

---

## Scope

This security policy applies to:
- AutoMapper
- MediatR
- Other libraries maintained by Lucky Penny Software LLC

These libraries run within customer-controlled environments. Customers are responsible for securing their own applications and infrastructure.

---

## Additional Information

Lucky Penny Software LLC does not maintain formal certifications such as SOC 2 or ISO 27001. Security practices are implemented in alignment with industry best practices and are appropriate to the company’s size and service model.
