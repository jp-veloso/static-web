export type AccessToken = {
    exp: number;
    username: string;
    id: number;
    roles: string[];
    dep: string;
}

export type User = {
    token: string;
    picture: string;
    name: string;
    department: string;
}

export type Kpis = {
    ltv: number,
    recipe: number,
    nps: number,
    cac: number,
    clients: number
}

export type PagedResponse = {
    data: ClientModel[];
    totalPages: number;
    totalElements: number;
    pageNumber: number;
    pageSize: number;
}

export type ClientModel = {
    id: number;
    cnpj: string;
    name: string;
    segment: string;
    createdAt: string;
}

export type ClientInfo = {
    id: number,
    cnpj: string,
    name: string,
    segment: string,
    createdAt: Date,
    active: boolean,
    isClient: boolean,
    pipeId?: string
}

export type Issue = {
    id: number,
    bounty: number,
    commission: number,
    validity?: number,
    dealId?: number,
    policyId: string,
    insuredCnpj?: string,
    issuedAt: string,
    product: string,
    validUntil: Date,
    value: number,
    isPaid: boolean,
    lastRate: number,
    insurer: Insurer
    users: string[]
}

export type Insurer = {
    id: number,
    name: string,
    hasIntegration: boolean,
    picture?: string,
    active: boolean,
    parameters?: Parameters[]
}

export type Parameters = {
    proposalType: ContractType;
    ccg: number;
    minimumBrokerage: number;
    internalRetroactivity: boolean;
    externalRetroactivity: number;
    exclusive: boolean;
    pstp: boolean;
    baseCommission: number;
    maximumCommission: number;
    minimumBounty: number;
    grievanceRule: string;
}

export type Taker = {
    id: number,
    category: string,
    balance: number,
    limit: number,
    rate: number
}

export type EnrollmentType = {
    createdAt: string,
    expireAt?: string,
    status: string,
    warn?: string,
    rating?: string,
    isActive: boolean,
    insurer: Insurer,
    insurerId?: number,
    takers: Taker[]
    virtualRates: VirtualRate[];
}

export type VirtualRate = {
    virtualRate: number;
    rating: string;
    ratingSource: string;
}

export type PostClient = {
    cnpj: string;
    name?: string;
    segment?: string;
    sync: boolean;
}

export type ProposalItemType = {
    bounty: number;
    commission: number;
    rate: number;
    status: string;
    balance: number;
    insurer: Insurer;
    warns: Hash;
}

export type Hash = {
    [key: string]: boolean;
}

export type ContractType = "PRIVATE_CONTRACT" | "PUBLIC_CONTRACT";