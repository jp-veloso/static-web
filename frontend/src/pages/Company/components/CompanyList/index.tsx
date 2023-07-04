import CompanyCard from "../CompanyCard";
import "../../style.css"
import { useCallback, useEffect, useState } from "react";
import { ContractType, Insurer } from "../../../../core/types";
import { makePrivateRequest } from "../../../../core/script";

const CompanyList = () => {
    const [selectedType, setSelectedType] = useState<ContractType>("PUBLIC_CONTRACT")
    const [insurers, setInsurers] = useState<Insurer[]>()

    const getInsurers = useCallback(() => {
        makePrivateRequest({url: '/insurers', params: { type: selectedType }}).then(r => setInsurers(r.data))
    },[selectedType])

    useEffect(() => {
        getInsurers()
    },[getInsurers])

    return <div className="h-[calc(100vh-4rem)] overflow-y-scroll scrollbar">
        <div className="text-center">
            <h1 className="font-bold text-neutral-900 text-5xl my-4">Companhias</h1>
            <p className="text-neutral-400 text-2xl font-medium">Abaixo estão todas as informações referentes as regras de aceitação das seguradoras. <br /> Altere entre público e privado de acordo com o tipo do contrato.</p>
        </div>
        <div className="mt-2.5 text-center">
            <button className={`${selectedType === 'PUBLIC_CONTRACT' ? 'company-button-first' : 'company-button-unselected'} mr-5 w-64 p-2.5`} onClick={() => setSelectedType('PUBLIC_CONTRACT')}>Público</button>
            <button className={`${selectedType === 'PRIVATE_CONTRACT' ? 'company-button-first' : 'company-button-unselected'} w-64 p-2.5`} onClick={() => setSelectedType('PRIVATE_CONTRACT')}>Privado</button>
        </div>
        <div className="flex flex-wrap justify-center">
            {insurers && insurers.map(x => (
                <CompanyCard key={x.id} insurer={x}/>
            ))}
        </div>     
    </div>
}

export default CompanyList;