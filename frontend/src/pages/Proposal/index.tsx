import { useCallback, useEffect, useState } from "react"
import { useParams } from "react-router-dom"
import ProposalForm from "./components/ProposalForm"
import ProposalItem from "./components/ProposalItem"
import { formatCurrency, makePrivateRequest } from "../../core/script"
import { ClientInfo, ProposalItemType, } from "../../core/types"
import ClientCard from "../Client/components/ClientCard"
import { useShowModal } from "../Client"
import _minLogo from '../../assets/_minLogo.svg'
import _warn from "../../assets/information-circle.svg"
import _cash from '../../assets/cash.png'

const Proposal = () => {
    const { id } = useParams()
    const { onModalChange } = useShowModal()
    const [client, setClient] = useState<ClientInfo>()
    const [proposals, setProposals] = useState<ProposalItemType[]>([])
    const [defaultProposal, setDefaultProposal] = useState<ProposalItemType>()

    const getClient = useCallback(() => {
        makePrivateRequest({ url: `/clients/${id}` })
            .then(response => { setClient(response.data) })
    }, [])

    const handleProposals = (items: ProposalItemType[]) => {
        setProposals(items.filter(x => x.insurer))
        setDefaultProposal(items.find(x => x.insurer === undefined))

    }

    useEffect(() => {
        getClient()
    }, [])


    return <div className="h-[calc(100vh-4rem)] overflow-y-scroll scrollbar flex relative">
        <div className="w-96"></div>
        {client &&
            <>
                <ClientCard client={client} modalFunc={onModalChange} />
                <div className="basis-full mr-1 h-fit">
                    <ProposalForm id={Number(id)} setProposals={handleProposals} />
                    <div className="flex flex-wrap justify-center">
                        {proposals && proposals.filter(x => x.insurer).map(x => (
                            <ProposalItem item={x} key={x.insurer.id} />
                        ))}
                    </div>
                    {defaultProposal && (
                        <div className="flex justify-around my-2 mx-3 p-2 items-center shadow-md rounded-xl bg-white">
                            <div className='flex items-center justify-center rounded-full w-14 h-14 shadow-md bg-white overflow-hidden'>
                                <img src={_minLogo} alt="minLogo" style={{ width: '40px', height: '40px' }} />
                            </div>
                            <div className="basis-2/5">
                                <p className="flex items-center text-neutral-900 text-sm font-medium"> <img src={_cash} className="mr-2"></img> Prêmio Geral</p>
                                <span className="text-2xl font-semibold">{formatCurrency(defaultProposal.bounty)}</span>
                            </div>
                            <div className="basis-2/5 text-sm">
                                <div className="flex">
                                    <img className='mr-1' src={_warn}></img>
                                    <span className="font-semibold">Avisos</span>
                                </div>
                                {Object.keys(defaultProposal.warns).map(x => (
                                    <p key={x}><span className="font-semibold text-amber-600" >{x}:</span> {defaultProposal.warns[x]}</p>
                                ))
                                }
                            </div>
                        </div>
                    )}
                </div>
            </>
        }
    </div>
}

export default Proposal