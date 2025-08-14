import { jwtDecode } from 'jwt-decode'

export function getCurrentUserId(): string | null {
	try {
		const t = localStorage.getItem('access_token')
		if (!t) return null
		const payload = jwtDecode<{ sub?: string }>(t)
		return payload?.sub ?? null
	} catch {
		return null
	}
}


