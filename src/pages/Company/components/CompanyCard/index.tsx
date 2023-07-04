import '../../style.css'
import _mock from '../../../../assets/mock.png'
import { Insurer, Parameters } from '../../../../core/types'
import { formatCurrency, isAllowedByRole } from '../../../../core/script';
import { useNavigate } from 'react-router-dom';

type Props = {
    insurer: Insurer;
}

const CompanyCard = ({ insurer }: Props) => {
    const nav = useNavigate(); 

    return <div className="bg-white drop-shadow-lg rounded-lg max-w-xl p-4 flex m-2.5 basis-full">
        <div className='w-1/5 mr-2.5 flex flex-col justify-between'>
            <div className="flex justify-center items-center w-auto shadow-md rounded-lg h-20 overflow-hidden">
                {insurer.picture ? <img src={insurer.picture}></img> : <span>{insurer.name}</span>}
            </div>
            {isAllowedByRole(["admin"]) && (<button className="p-2 company-button-first block w-full" onClick={() => nav(`/companies/${insurer.id}/${insurer.parameters![0].proposalType === 'PUBLIC_CONTRACT' ? 'public' : 'private'}`)}>Editar</button>)}
        </div>
        {insurer.parameters && (
            <div className={`w-4/5 grid ${insurer.parameters[0].proposalType === 'PUBLIC_CONTRACT' ? 'grid-rows-5' : 'grid-rows-4'} gap-1 grid-flow-col`}>
                <label className="text-sm">
                    <p className="text-neutral-400">CCG</p>
                    <p className="text-neutral-900 font-bold">{insurer.parameters[0].ccg == 0 ? 'Sob análise' : formatCurrency(insurer.parameters[0].ccg)}</p>
                </label>
                <label className="text-sm">
                    <p className="text-neutral-400">Retroatividade portal</p>
                    <p className="text-neutral-900 font-bold">{insurer.parameters[0].externalRetroactivity} dias</p>
                </label>
                <label className="text-sm">
                    <p className="text-neutral-400">Dedicação exclusiva</p>
                    <p className="text-neutral-900 font-bold">{insurer.parameters[0].exclusive ? 'Sim' : 'Não'}</p>
                </label>
                <label className="text-sm">
                    <p className="text-neutral-400">Comissão base</p>
                    <p className="text-neutral-900 font-bold">{(insurer.parameters[0].baseCommission * 100).toFixed(2)}%</p>
                </label>
                <label className="text-sm">
                    <p className="text-neutral-400">Prêmio mínimo</p>
                    <p className="text-neutral-900 font-bold">{formatCurrency(insurer.parameters[0].minimumBounty)}</p>
                </label>
                <label className="text-sm">
                    <p className="text-neutral-400">Prazo de transferência</p>
                    <p className="text-neutral-900 font-bold">Até {insurer.parameters[0].minimumBrokerage} horas</p>
                </label>
                <label className="text-sm">
                    <p className="text-neutral-400">Retroatividade interna</p>
                    <p className="text-neutral-900 font-bold">{insurer.parameters[0].internalRetroactivity ? 'Aceita com DBA' : 'Não há'}</p>
                </label>
                {insurer.parameters[0].proposalType === 'PUBLIC_CONTRACT' && (
                    <label className="text-sm">
                        <p className="text-neutral-400">Prestação de Serviços</p>
                        <p className="text-neutral-900 font-bold">{insurer.parameters[0].pstp ? 'Sim' : 'Não'}</p>
                    </label>
                )}
                <label className="text-sm">
                    <p className="text-neutral-400">Comissão máxima</p>
                    <p className="text-neutral-900 font-bold">{(insurer.parameters[0].maximumCommission * 100).toFixed(2)}%</p>
                </label>
                {insurer.parameters[0].proposalType === 'PUBLIC_CONTRACT' && (
                    <label className="text-sm">
                        <p className="text-neutral-400">Agravo trabalhista</p>
                        <p className="text-neutral-900 font-bold">{insurer.parameters[0].grievanceRule}</p>
                    </label>
                )}
            </div>
        )}
    </div>
}

export default CompanyCard;