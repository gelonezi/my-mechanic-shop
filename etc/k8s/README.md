# Home server (k3s)

The `Docker images` workflow deploys `main` here: the self-hosted runner `home-k3s` applies this
folder with every image pinned to the commit SHA. Public URLs, through a Cloudflare Tunnel
(cloudflared on the host, both hostnames routed to `http://localhost:80`, which is Traefik):

- <https://mymechanicshop.gelonezi.com> — Angular
- <https://mymechanicshop-api.gelonezi.com> — API, login, Swagger

## One-time cluster setup

As the cluster admin on the server:

```bash
kubectl create namespace mymechanicshop
kubectl apply -f cluster/   # the runner's namespace-scoped ServiceAccount + Traefik trusting X-Forwarded-Proto
```

Secrets are created on the cluster, never committed. Generated values nobody needs to know:

```bash
umask 077; P=$(openssl rand -hex 24)
openssl req -x509 -newkey rsa:2048 -nodes -keyout key.pem -out cert.pem -days 3650 -subj "/CN=MyMechanicShop OpenIddict"
openssl pkcs12 -export -inkey key.pem -in cert.pem -out openiddict.pfx -passout "pass:$P"
kubectl -n mymechanicshop create secret generic openiddict-certificate --from-file=openiddict.pfx
kubectl -n mymechanicshop create secret generic mymechanicshop \
  --from-literal=postgres-password="$(openssl rand -hex 24)" \
  --from-literal=openiddict-passphrase="$P" \
  --from-literal=string-encryption-passphrase="$(openssl rand -hex 24)"
unset P; rm key.pem cert.pem openiddict.pfx
```

The host admin's password, seeded on a new database only (afterwards, change it in the app):

```bash
read -rsp 'Admin password: ' A && echo && kubectl -n mymechanicshop create secret generic mymechanicshop-admin --from-literal=password="$A"; unset A
```

The runner's kubeconfig is the `deployer` ServiceAccount's token, in
`/home/gha-runner/.kube/config`, and `KUBECONFIG` points at it in the runner's `.env`.
